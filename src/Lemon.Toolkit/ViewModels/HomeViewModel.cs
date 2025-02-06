using Lemon.ModuleNavigation.Abstracts;
using Lemon.ModuleNavigation.Core;
using Lemon.Toolkit.Domains;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace Lemon.Toolkit.ViewModels
{
    public class HomeViewModel : NavigationViewModelBase
    {
        private readonly ITopLevelProvider _topLevelProvider;
        private readonly INavigationService _navigationService;
        private readonly ILogger _logger;
        private readonly IEnumerable<string> _requestNewViews;
        public HomeViewModel(ITopLevelProvider topLevelProvider,
            IRegionManager regionManager,
            INavigationService navigationService,
            ILogger<HomeViewModel> logger)
        {
            _navigationService = navigationService;
            _topLevelProvider = topLevelProvider;
            _logger = logger;
            _requestNewViews = ["ToolBoxView"];

            ActivateViewCommand = ReactiveCommand.Create<ViewDiscription>(v => 
            {
                if (!string.IsNullOrEmpty(v.ViewKey))
                {
                    _navigationService.RequestViewNavigation("MainTabRegion", v.ViewKey, !_requestNewViews.Contains(v.ViewKey));
                }
            });

            Views = ViewManager.ViewDiscriptions.Values.Where(v=>v.ViewModelType != typeof(HomeViewModel));
            _logger.LogDebug($"{string.Join(';', Views.Select(v => v.ViewKey))}");
        }
        public ReactiveCommand<ViewDiscription,Unit> ActivateViewCommand { get; }
        public IEnumerable<ViewDiscription> Views
        {
            get;
        }
    }
}
