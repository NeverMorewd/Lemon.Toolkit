using Avalonia;
using Avalonia.ReactiveUI;
using Lemon.Hosting.AvaloniauiDesktop;
using Lemon.ModuleNavigation;
using Lemon.ModuleNavigation.Avaloniaui;
using Lemon.ModuleNavigation.Avaloniaui.Extensions;
using Lemon.Toolkit.Domains;
using Lemon.Toolkit.Logging;
using Lemon.Toolkit.Models;
using Lemon.Toolkit.Services;
using Lemon.Toolkit.Shells;
using Lemon.Toolkit.ViewModels;
using Lemon.Toolkit.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.Versioning;

namespace Lemon.Toolkit
{
    internal class Program
    {
        internal readonly static ConsoleService _consoleService = new();
        [STAThread]
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        public static void Main(string[] args)
        {
            Console.SetOut(_consoleService);
            Console.SetError(_consoleService);
            Console.WriteLine("====𝕃𝕖𝕞𝕠𝕟====");
            var hostBuilder = Host.CreateApplicationBuilder();

            // config IConfiguration
            hostBuilder.Configuration
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .AddInMemoryCollection();

            // logger
            hostBuilder.Logging.ClearProviders();
            hostBuilder.Services.AddLogging(builder =>
            {
                var miniLevel = LogLevel.Debug;
                builder.SetMinimumLevel(miniLevel);
                builder.AddProvider(new UILoggerProvider("UILogger",
                    _consoleService,
                    miniLevel));
            });

            // navigation
            hostBuilder.Services.AddAvaNavigationSupport();
            // view
            hostBuilder.Services.AddView<FileInspectorView, FileInspectorViewModel>(nameof(FileInspectorView));
            hostBuilder.Services.AddView<HomeView, HomeViewModel>(nameof(HomeView));
            hostBuilder.Services.AddView<TestView, TestViewModel>(nameof(TestView));
            hostBuilder.Services.AddView<ToolBoxView, ToolBoxViewModel>(nameof(ToolBoxView));
            hostBuilder.Services.AddView<ChromePreferenceInspector, ChromePreferenceViewModel>(nameof(ChromePreferenceInspector));

            // services
            hostBuilder.Services.AddSingleton(_consoleService);
            hostBuilder.Services.AddSingleton<FileInspectorService>();
            hostBuilder.Services.AddSingleton<ITopLevelProvider, TopLevelProvider>();
            hostBuilder.Services.AddSingleton<ShellService>();
            hostBuilder.Services.AddSingleton<GitSettingsService>();
            hostBuilder.Services.AddSingleton<IObservable<ShellParamModel>>(sp => sp.GetRequiredService<ShellService>());
            hostBuilder.Services.AddSingleton<IObserver<ShellParamModel>>(sp => sp.GetRequiredService<ShellService>());
            //
            hostBuilder.Services.AddAvaloniauiDesktopApplication<App>(ConfigAvaloniaAppBuilder);
            hostBuilder.Services.AddMainWindow<MainWindow, MainWindowViewModel>();
            RunApp(hostBuilder, args);

        }
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        [SupportedOSPlatform("macos")]
        private static void RunApp(HostApplicationBuilder hostBuilder, string[] args)
        {
            var appHost = hostBuilder.Build();
            appHost.RunAvaloniauiApplication<MainWindow>(args);
        }
        public static AppBuilder ConfigAvaloniaAppBuilder(AppBuilder appBuilder)
            => appBuilder
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}
