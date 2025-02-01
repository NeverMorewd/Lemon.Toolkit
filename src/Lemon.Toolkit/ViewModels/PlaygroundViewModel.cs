using Lemon.Toolkit.Services.OllamaServices;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Lemon.Toolkit.ViewModels
{
    public class PlaygroundViewModel : NavigationViewModelBase
    {
        private readonly OllamaServiceFacade _serviceFacade;
        private readonly ILogger _logger;

        public PlaygroundViewModel(OllamaServiceFacade ollamaServiceFacade, ILogger<PlaygroundViewModel> logger)
        {
            _serviceFacade = ollamaServiceFacade;
            _logger = logger;
            AskCommand = ReactiveCommand.CreateFromTask<string?, string>(AskAsync);
            AskCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(reply =>
            {
                ReplyContent = reply;
            });
        }

        private async Task<string> AskAsync(string? arg)
        {
            if (!string.IsNullOrEmpty(arg))
            {
                return await _serviceFacade.Ask(arg);
            }

            return "null";
        }

        public ReactiveCommand<string?, string> AskCommand { get; }
        [Reactive]
        public string ReplyContent
        {
            get;
            set;
        }
    }
}
