using Lemon.ModuleNavigation.Abstracts;
using Lemon.ModuleNavigation.Core;

namespace Lemon.Toolkit.ViewModels
{
    public class FileSimulateViewModel : NavigationViewModelBase, INavigationAware
    {
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new System.NotImplementedException();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            throw new System.NotImplementedException();
        }
    }
}
