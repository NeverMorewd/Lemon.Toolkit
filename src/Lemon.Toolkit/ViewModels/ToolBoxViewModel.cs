using Lemon.Toolkit.Services;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Lemon.Toolkit.ViewModels
{
    public class ToolBoxViewModel : NavigationViewModelBase
    {
        private readonly GitSettingsService _gitSettingsService;
        private readonly ILogger _logger;

        public ToolBoxViewModel(GitSettingsService gitSettingsService, ILogger<ToolBoxViewModel> logger)
        {
            var sw = Stopwatch.StartNew(); 
            _gitSettingsService = gitSettingsService;
            _logger = logger;
            GetProxySettingsCommand = ReactiveCommand.CreateFromTask<Unit, string>(_ =>
            {
                return Task.Run(() =>
                {
                    var proxyInfo = _gitSettingsService.GetCurrentProxy();
                    return proxyInfo;
                });
            });
            GetProxySettingsCommand
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(info => 
                {
                    CurrentGitProxyInfo = info;
                });
            this.WhenAnyValue(t => t.EnableGitProxy)
                .Skip(1)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .Subscribe(next =>
                {
                    if (next)
                    {
                        _gitSettingsService.EnableProxy($"http://127.0.0.1:{GitProxyPort}");
                    }
                    else
                    {
                        _gitSettingsService.DisableProxy();
                    }
                });

            //this.WhenAnyValue(t => t.GitProxyPort)
            //    .Skip(1)
            //    .Where(_ => EnableGitProxy)
            //    .Subscribe(port =>
            //    {
            //        _gitSettingsService.EnableProxy($"http://127.0.0.1:{port}");
            //    });
            GetProxySettingsCommand.Execute().Subscribe();
            Task.Run(() =>
            {
                var proxy = _gitSettingsService.GetCurrentProxy();
                _logger.LogInformation($"Current proxy: {proxy}");
            });
            _logger.LogInformation($"ToolBoxViewModel:{sw.ElapsedMilliseconds}ms");
        }

        [Reactive]
        public int GitProxyPort { get; set; } = 7078;

        [Reactive]
        public bool EnableGitProxy { get; set; } = false;

        [Reactive]
        public string? CurrentGitProxyInfo { get; set; }

        public ReactiveCommand<Unit,string> GetProxySettingsCommand { get; set; }
    }
}