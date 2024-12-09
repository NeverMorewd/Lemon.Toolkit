using DynamicData.Binding;
using Lemon.ModuleNavigation.Abstracts;
using Lemon.ModuleNavigation.Core;
using Lemon.Toolkit.Domains;
using Microsoft.Extensions.Logging;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemon.Toolkit.ViewModels
{
    public class HomeViewModel : NavigationViewModelBase
    {
        private readonly ITopLevelProvider _topLevelProvider;
        private readonly INavigationService _navigationService;
        private readonly ILogger _logger;
        public HomeViewModel(ITopLevelProvider topLevelProvider,
            IRegionManager regionManager,
            INavigationService navigationService,
            ILogger<HomeViewModel> logger)
        {
            _navigationService = navigationService;
            _topLevelProvider = topLevelProvider;
            _logger = logger;
            Views = ViewManager.ViewDiscriptions.Values.Where(v=>v.ViewModelType != typeof(HomeViewModel));
            using var scope = _logger.BeginScope("Views");
            _logger.LogDebug($"{string.Join(';', Views.Select(v => v.ViewKey))}");

            this.WhenPropertyChanged(t => t.SelectedView)
                .Subscribe((newValue) =>
                {
                    if (newValue.Value.HasValue && !string.IsNullOrEmpty(newValue.Value.Value.ViewKey))
                    {
                        _navigationService.RequestViewNavigation("MainTabRegion", newValue.Value.Value.ViewKey, true);
                        GoClearSelection = true;
                        SelectedView = null;
                    }
                });

        }

        public IEnumerable<ViewDiscription> Views
        {
            get;
        }

        [Reactive]
        public bool GoClearSelection
        {
            get;
            set;
        } = false;
        [Reactive]
        public ViewDiscription? SelectedView
        {
            get;
            set;
        }
    }
}
