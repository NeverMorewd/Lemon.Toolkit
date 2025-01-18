using Lemon.ModuleNavigation.Abstracts;
using Lemon.ModuleNavigation.Core;
using ReactiveUI;

namespace Lemon.Toolkit.ViewModels
{
    public class NavigationViewModelBase : ReactiveObject, INavigationAware
    {
        public virtual void Dispose()
        {

        }

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }
    }
}
