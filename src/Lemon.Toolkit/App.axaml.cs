using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;

namespace Lemon.Toolkit
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
           //
        }
    }
}