using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Lemon.ModuleNavigation.Abstracts;
using Lemon.Toolkit.Services;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Reactive;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Lemon.Toolkit.ViewModels
{
    [SupportedOSPlatform("windows")]
    public class TestViewModel : NavigationViewModelBase
    {
        private static readonly TextBlock _text = new() { Text="I am static!" };
        private readonly ILogger _logger;
        private readonly WindowsFeatureService _windowsFeatureService;
        public TestViewModel(WindowsFeatureService windowsFeatureService,ILogger<TestViewModel> logger)
        {
            TestCommand = ReactiveCommand.CreateFromTask(TestCommandAsync);
            _windowsFeatureService = windowsFeatureService;
            _logger = logger;
        }

        private async Task TestCommandAsync()
        {
            await Task.Run(() => 
            {
                try
                {
                    var can = _windowsFeatureService.CanRegisterTask();
                    _logger.LogDebug($"CanRegisterTask:{can}");
                    if (can)
                    {
                        _windowsFeatureService.CreateTask($"Lemon.Test", Environment.ProcessPath!, "Lemon", 1);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "TestCommandAsync");
                    if (ex.InnerException is not null)
                    {
                        _logger.LogError(ex.InnerException, "TestCommandAsync");
                    }
                }
            });
        }

        public IDataTemplate TestNewTemplate
        => new FuncDataTemplate<string>((x, __) =>
        {
            return new TextBlock { Text = $"test:{x}" };
        });

        public IDataTemplate TestTemplate
        => new FuncDataTemplate<string>((_, __) =>
        {
            return _text;
        });

        public ReactiveCommand<Unit, Unit> TestCommand { get; }
    }
}
