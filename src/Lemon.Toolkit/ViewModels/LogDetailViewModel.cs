using Lemon.HandyLib.Logging.Definitions;
using Lemon.ModuleNavigation.Abstracts;
using ReactiveUI.Fody.Helpers;
using System;

namespace Lemon.Toolkit.ViewModels
{
    internal class LogDetailViewModel : NavigationViewModelBase, IDialogAware
    {
        public string Title => nameof(LogDetailViewModel);

        public event Action<IDialogResult>? RequestClose;

        [Reactive]
        public LogEntry? Log
        { get; set; }

        public void OnDialogClosed()
        {
            //
        }

        public void OnDialogOpened(IDialogParameters? parameters)
        {
            if(parameters!.TryGetValue("log",out LogEntry? logEntry))
            {
                if (logEntry != null)
                {
                    Log = logEntry;
                }
            }
        }
    }
}
