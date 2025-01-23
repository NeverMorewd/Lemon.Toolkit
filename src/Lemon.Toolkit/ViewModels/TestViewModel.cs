using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Lemon.ModuleNavigation.Abstracts;
using Lemon.Toolkit.Services;
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

        private readonly WindowsFeatureService _windowsFeatureService;
        public TestViewModel(WindowsFeatureService windowsFeatureService)
        {
            TestCommand = ReactiveCommand.CreateFromTask(TestCommandAsync);
            _windowsFeatureService = windowsFeatureService;
        }

        private async Task TestCommandAsync()
        {
            await Task.Run(() => 
            {
                _windowsFeatureService.CreateTask($"Lemon.Test",Environment.ProcessPath!,"Lemon",1);
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
