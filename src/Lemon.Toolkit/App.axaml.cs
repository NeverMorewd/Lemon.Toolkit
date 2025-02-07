using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Lemon.Toolkit
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public App(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetRequiredService<ILogger<App>>();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public override void OnFrameworkInitializationCompleted()
        {
            base.OnFrameworkInitializationCompleted();
            Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;
        }

        private void UIThread_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.LogError(e.Exception.ToString());
        }
    }
}