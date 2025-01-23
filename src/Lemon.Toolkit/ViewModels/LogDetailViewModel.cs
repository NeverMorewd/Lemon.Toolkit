using Lemon.ModuleNavigation.Abstracts;
using System;

namespace Lemon.Toolkit.ViewModels
{
    internal class LogDetailViewModel : NavigationViewModelBase, IDialogAware
    {
        public string Title => nameof(LogDetailViewModel);

        public event Action<IDialogResult>? RequestClose;

        public void OnDialogClosed()
        {
            //
        }

        public void OnDialogOpened(IDialogParameters? parameters)
        {
            //
        }
    }
}
