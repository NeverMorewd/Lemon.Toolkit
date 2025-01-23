using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using DynamicData;
using Lemon.HandyLib.Logging.Definitions;
using Lemon.ModuleNavigation.Abstracts;
using Lemon.ModuleNavigation.Core;
using Lemon.Toolkit.Domains;
using Lemon.Toolkit.Models;
using Lemon.Toolkit.ViewModels;
using Lemon.Toolkit.Views;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Notification = Avalonia.Controls.Notifications.Notification;

namespace Lemon.Toolkit.Shells
{
    public class MainWindowViewModel : NavigationViewModelBase, IServiceAware, IDisposable
    {
        private const int MaxOutputCount = 200;
        private readonly CompositeDisposable _disposables;
        private readonly ITopLevelProvider _topLevelProvider;
        private readonly ConsoleStreamService _consoleService;
        private readonly IObservable<ShellParamModel> _shellService;
        private readonly ILogger _logger;
        private readonly SourceCache<LogEntry, Guid> _outputsCache = new(x => x.Id);
        private readonly ReadOnlyObservableCollection<LogEntry> _outputs;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        public MainWindowViewModel(ITopLevelProvider topLevelProvider,
            ConsoleStreamService consoleService,
            IObservable<ShellParamModel> shellService,
            INavigationService navigationService,
            IServiceProvider serviceProvider,
            IDialogService dialogService,
            ILogger<MainWindowViewModel> logger)
        {
            _logger = logger;
            _dialogService = dialogService;
            _topLevelProvider = topLevelProvider;
            _consoleService = consoleService;
            _shellService = shellService;
            _navigationService = navigationService;
            ServiceProvider = serviceProvider;
            _navigationService.RequestViewNavigation("MainTabRegion", nameof(HomeView));

            #region Outputs Cache
            var cacheCleanup = _outputsCache.Connect()
                .LimitSizeTo(MaxOutputCount)
                .Bind(out _outputs)
                .DisposeMany()
                .Subscribe();

            var cacheCountCleanup = _outputsCache.Connect()
                .CountChanged()
                .Subscribe(cache =>
                {
                    if (ConsoleIsExpanded) return;
                    if (OutputCount == 100) return;
                    OutputCount += cache.Count;
                });

            #endregion
            _shellService
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(param =>
                {
                    IsProcessing = param.IsProcessing;
                });
            ClearOutputCommand = ReactiveCommand.Create(() => _outputsCache.Clear());
            CopyOutputCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                if (_outputs.Count < 1)
                {
                    return;
                }
                var texts = _outputs.Select(o => o.Message);
                var outputString = string.Join(Environment.NewLine, texts);
                await _topLevelProvider.Ensure().Clipboard!.SetTextAsync(outputString);
                _topLevelProvider.NotificationManager!.Show(new Notification("Success", "Copied!", NotificationType.Success));
            });
            ExpandCommand = ReactiveCommand.Create<LogEntry>(ShowLogDetails);
            ExecuteCommand = ReactiveCommand.CreateFromTask<string?>(CommandExecuteAync);
            var valueChangedCleanup = this.WhenAnyValue(x => x.ConsoleIsExpanded)
                .Subscribe(c =>
                {
                    if (c)
                    {
                        OutputCount = 0;
                    }
                });
            _consoleService
                 .OutputStream
                 .Merge(_consoleService.ErrorStream)
                 .Select(line => LogEntry.ParseLog(line, threadId: Environment.CurrentManagedThreadId))
                 .ObserveOn(RxApp.MainThreadScheduler)
                 .Subscribe(
                     log =>
                     {
                         _outputsCache.AddOrUpdate(log);
                     }
                 );
            _disposables = new(cacheCleanup,
                cacheCountCleanup,
                valueChangedCleanup);

        }
        [Reactive]
        public bool IsProcessing
        {
            get;
            set;
        }
        [Reactive]
        public int OutputCount
        {
            get;
            set;
        }
        [Reactive]
        public bool ConsoleIsExpanded
        {
            get;
            set;
        }
        [Reactive]
        public IModule? CurrentTab
        {
            get;
            set;
        }
        public ReactiveCommand<LogEntry, Unit> ExpandCommand { get; }
        public ReactiveCommand<string?, Unit> ExecuteCommand { get; }
        public ReadOnlyObservableCollection<LogEntry> LogEntries
        {
            get => _outputs;
        }
        public ReactiveCommand<Unit, Unit> ClearOutputCommand
        {
            get;
        }
        public ReactiveCommand<Unit, Unit> CopyOutputCommand
        {
            get;
        }

        public IServiceProvider ServiceProvider
        {
            get;
        }

        private void ShowLogDetails(LogEntry logEntry)
        {
            _dialogService.Show(nameof(LogDetailView), nameof(CRTWindow));
            //var dialog = new CRTWindow
            //{
            //    Title = "Log Details",
            //    Width = 600,
            //    Height = 400,
            //    WindowState = WindowState.Maximized,
            //    WindowStartupLocation = WindowStartupLocation.CenterOwner,
            //    Content = new StackPanel
            //    {
            //        Spacing = 2,
            //        Orientation = Avalonia.Layout.Orientation.Vertical,
            //        Margin = new Thickness(10),
            //        Children =
            //        {
            //            new TextBlock { Text = $"Timestamp: {logEntry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}" },
            //            new TextBlock { Text = $"Level: {logEntry.Level}" },
            //            new TextBlock { Text = $"Process ID: {logEntry.ProcessId}" },
            //            new TextBlock { Text = $"Thread ID: {logEntry.ThreadId}" },
            //            new TextBlock { Text = $"Interval: {logEntry.Interval}" },
            //            new TextBlock { Text = $"Caller: {logEntry.Caller}" },
            //            new TextBlock { Text = $"Message: {logEntry.Message}" },
            //            new TextBlock { Text = $"Exception: {logEntry.Exception}" }
            //        }
            //    }
            //};
            //dialog.ShowDialog(_topLevelProvider.MainWindow);
        }

        private Task CommandExecuteAync(string? commandLine)
        {
            var timeStamp = DateTime.Now;
            Console.WriteLine($"CommandExecuteAync:{timeStamp}");
            return Task.Run(() =>
            {
                if (commandLine == null)
                {
                    return;
                }

                _outputsCache.AddOrUpdate(LogEntry.ParseLog(commandLine,
                    timeStamp,
                    Environment.CurrentManagedThreadId, 
                    LogEntryType.ConsoleIn));

                Console.WriteLine($"handle:{commandLine}");
            });
        }
        public override void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}
