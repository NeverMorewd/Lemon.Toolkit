using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using DynamicData.Binding;
using Lemon.Toolkit.Domains;
using Lemon.Toolkit.Models;
using Lemon.Toolkit.Models.Ollama;
using Lemon.Toolkit.Services;
using Lemon.Toolkit.Services.OllamaServices;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Refit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace Lemon.Toolkit.ViewModels
{
    public class PlaygroundViewModel : NavigationViewModelBase
    {
        private readonly OllamaServiceFacade _serviceFacade;
        private readonly ILogger _logger;
        private readonly IObserver<ShellParamModel> _shellService;
        private readonly ITopLevelProvider _topLevelProvider;
        public PlaygroundViewModel(OllamaServiceFacade ollamaServiceFacade,
            ITopLevelProvider topLevelProvider,
            IObserver<ShellParamModel> shellService,
            ILogger<PlaygroundViewModel> logger)
        {
            _serviceFacade = ollamaServiceFacade;
            _shellService = shellService;
            _topLevelProvider = topLevelProvider;
            _logger = logger;
            var canExecuteObservable = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
                                        .Select(_ => !IsOllamaProcessRunning())
                                        .DistinctUntilChanged()
                                        .ObserveOn(RxApp.MainThreadScheduler);
            MockCommand = ReactiveCommand.CreateFromTask<OllamaApiMeta>(MockAsync);
            AskCommand = ReactiveCommand.CreateFromTask<string?, string>(AskAsync);
            AskCommand
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(reply =>
                {
                    ReplyContent = reply;
                });
            Type apiType = typeof(IOllamaApi);
            var methodInfos = apiType.GetMethods();
            ApiCollection = new ObservableCollection<OllamaApiMeta>(methodInfos.Select(m => 
            {
                var httpmethod = m.GetCustomAttributes(typeof(HttpMethodAttribute), true);
                return (httpmethod.FirstOrDefault(), m);
            })
            .Where(v=>v.Item1 != null)
            .Select(v => 
            {
                var methodAttribute = (v.Item1 as HttpMethodAttribute)!;
                return new OllamaApiMeta
                {
                    Name = v.m.Name,
                    Path = methodAttribute.Path,
                    Verb = methodAttribute.Method.Method,
                };
            }));
            this.WhenAnyValue(vm => vm.CurrentModel)
                .Subscribe(async p => 
                {
                    if (_serviceFacade.CurrentModel != null)
                    {
                        await _serviceFacade.UnloadModel(_serviceFacade.CurrentModel);
                    }
                    _serviceFacade.CurrentModel = p;
                    if (_serviceFacade.CurrentModel != null)
                    {
                        await _serviceFacade.LoadModel(_serviceFacade.CurrentModel!);
                    }
                });
            OllamaSearchPathCommand = ReactiveCommand.CreateFromTask<Unit, string?>(OllamaSearchPathAsync);
            OllamaRunCommand = ReactiveCommand.CreateFromTask<string, Process>(OllamaRrunAsync, canExecuteObservable);
            OllamaRunCommand
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(p =>
                {
                    if (p != null && !p.HasExited)
                    {
                        if (Models == null || !Models.Any())
                        {
                            _ = TryLoadModelsFromOllama();
                        }
                    }
                });
            _ = TryLoadModelsFromOllama();
        }
        private bool IsOllamaProcessRunning()
        {
            var running = Process.GetProcesses().Any(p => p.ProcessName.Equals("ollama", StringComparison.OrdinalIgnoreCase));
            return running;
        }
        private async Task MockAsync(OllamaApiMeta meta)
        {
            var ret = await _serviceFacade.Mock(meta.Path);
            _logger.LogDebug($"MockAsync response:{JsonSerializer.Serialize(ret)}");
        }

        private async Task<string> AskAsync(string? arg)
        {
            if (!string.IsNullOrEmpty(arg))
            {
                return await _serviceFacade.Ask(arg);
            }

            return "null";
        }

        public ReactiveCommand<string?, string> AskCommand { get; }
        public ReactiveCommand<OllamaApiMeta, Unit> MockCommand { get; }
        public ReactiveCommand<Unit, string?> OllamaSearchPathCommand { get; }
        public ReactiveCommand<string, Process> OllamaRunCommand { get; }
        public ReactiveCommand<Unit, Unit> OllamaTerminateCommand { get; }
        [Reactive]
        public string? ReplyContent
        {
            get;
            set;
        }
        [Reactive]
        public string? OllamaPath
        {
            get;
            set;
        }
        [Reactive]
        public IEnumerable<string>? Models
        {
            get;
            set;
        }
        [Reactive]
        public string? CurrentModel
        {
            get;
            set;
        }
        public ObservableCollection<OllamaApiMeta> ApiCollection
        {
            get;
        }

        private async Task<Process> OllamaRrunAsync(string arg)
        {
            return await Task.Factory.StartNew(() =>
            {
                ProcessStartInfo startInfo = new()
                {
                    FileName = Path.Combine(arg, "ollama app.exe"),
                    CreateNoWindow = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                Process process = new()
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true,
                };
                process.Exited += (sender, args) =>
                {
                    _logger.LogDebug($"{process.ProcessName}-{process.Id}:{process.ExitCode}");
                };
                process.OutputDataReceived += (sender, args) => 
                {
                    _logger.LogDebug($"{process.ProcessName}-{process.Id}:{args.Data}");
                };
                process.ErrorDataReceived += (sender, args) => 
                {
                    _logger.LogError($"{process.ProcessName}-{process.Id}:{args.Data}");
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                return process;
            });
        }


        private async Task<string?> OllamaSearchPathAsync(Unit unit)
        {
            await Task.CompletedTask;
            return null;
        }

        private async Task TryLoadModelsFromOllama()
        {
            _shellService.OnNext(new ShellParamModel { IsProcessing = true });
            try
            {
                OllamaPath = await _serviceFacade.GetPath();
                Models = await _serviceFacade.GetAvailableModels();
                if (Models != null && Models.Any())
                {
                    CurrentModel = Models.First();
                }
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _topLevelProvider.NotificationManager!.Show(new Notification("Error", ex.Message, NotificationType.Error));
                }, DispatcherPriority.Send);
                //_topLevelProvider.NotificationManager!.Show(new Notification("Error", ex.Message, NotificationType.Error));

            }
            finally
            {
                _shellService.OnNext(new ShellParamModel { IsProcessing = false });
            }
        }
    }
}
