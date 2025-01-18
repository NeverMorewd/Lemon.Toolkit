using DynamicData.Binding;
using Lemon.Toolkit.Services;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
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
            _gitSettingsService = gitSettingsService;
            _logger = logger;
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

            this.WhenAnyValue(t => t.GitProxyPort)
                .Skip(1)
                .Where(_ => EnableGitProxy)
                .Subscribe(port =>
                {
                    _gitSettingsService.EnableProxy($"http://127.0.0.1:{port}");
                });

            Task.Run(() =>
            {
                var proxy = _gitSettingsService.GetCurrentProxy();
                _logger.LogInformation("Current proxy: {Proxy}", proxy);
            });
        }

        [Reactive]
        public int GitProxyPort { get; set; } = 7078;

        [Reactive]
        public bool EnableGitProxy { get; set; } = false;
    }
}