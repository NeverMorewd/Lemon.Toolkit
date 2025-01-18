using DynamicData.Binding;
using Lemon.Toolkit.Services;
using Microsoft.Extensions.Logging;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace Lemon.Toolkit.ViewModels
{
    public class ToolBoxViewModel: NavigationViewModelBase
    {
        private readonly GitSettingsService _gitSettingsService;
        public ToolBoxViewModel(GitSettingsService gitSettingsService,ILogger<ToolBoxViewModel> logger) 
        {
            _gitSettingsService = gitSettingsService;
            this.WhenPropertyChanged(t => t.EnableGitProxy)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(next => 
                {
                    if (next.Value)
                    {
                        _gitSettingsService.EnableProxy($"http://127.0.0.1:{GitProxyPort}");
                    }
                    else
                    {
                        _gitSettingsService.DisableProxy();
                    }
                });
        }
        [Reactive]
        public int GitProxyPort
        {
            get;
            set;
        } = 7078;
        [Reactive]
        public bool EnableGitProxy
        {
            get;
            set;
        } = false;
    }
}
