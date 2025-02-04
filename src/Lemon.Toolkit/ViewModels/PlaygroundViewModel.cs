using DynamicData.Binding;
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
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lemon.Toolkit.ViewModels
{
    public class PlaygroundViewModel : NavigationViewModelBase
    {
        private readonly OllamaServiceFacade _serviceFacade;
        private readonly ILogger _logger;
        private readonly IObserver<ShellParamModel> _shellService;

        public PlaygroundViewModel(OllamaServiceFacade ollamaServiceFacade,
            IObserver<ShellParamModel> shellService,
            ILogger<PlaygroundViewModel> logger)
        {
            _serviceFacade = ollamaServiceFacade;
            _shellService = shellService;
            _logger = logger;
            MockCommand = ReactiveCommand.CreateFromTask<OllamaApiMeta>(MockAsync);
            AskCommand = ReactiveCommand.CreateFromTask<string?, string>(AskAsync);
            AskCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(reply =>
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
            Task.Run(async () => 
            {
                _shellService.OnNext(new ShellParamModel { IsProcessing = true });
                OllamaPath = await _serviceFacade.GetPath();
                Models = await _serviceFacade.GetAvailableModels();
                if (Models != null && Models.Any())
                {
                    CurrentModel = Models.First();
                }
                _shellService.OnNext(new ShellParamModel { IsProcessing = false });
            });
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
    }
}
