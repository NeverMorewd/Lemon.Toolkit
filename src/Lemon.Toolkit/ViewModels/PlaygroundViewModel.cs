using Lemon.Toolkit.Models.Ollama;
using Lemon.Toolkit.Services.OllamaServices;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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
            MockCommand = ReactiveCommand.CreateFromTask<OllamaApiMeta>(MockAsync);
            AskCommand = ReactiveCommand.CreateFromTask<string?, string>(AskAsync);
            AskCommand.ObserveOn(RxApp.MainThreadScheduler).Subscribe(reply =>
            {
                ReplyContent = reply;
            });
            Type apiType = typeof(IOllamaApi);
            var methodInfos = apiType.GetMethods();
            ApiCollection = new ObservableCollection<OllamaApiMeta>(methodInfos.Select(m => 
            {
                var httpmethod = m.GetCustomAttributes(typeof(HttpMethodAttribute), true);
                return (httpmethod.FirstOrDefault(), m);
            })
            .Where(v=>v.Item1 != null)
            .Select(v => 
            {
                var methodAttribute = (v.Item1 as HttpMethodAttribute)!;
                return new OllamaApiMeta
                {
                    Name = v.m.Name,
                    Path = methodAttribute.Path,
                    Verb = methodAttribute.Method.Method,
                };
            }));
        }

        private async Task MockAsync(OllamaApiMeta meta)
        {
            var ret = await _serviceFacade.Mock(meta.Path);
            _logger.LogDebug($"MockAsync response:{JsonSerializer.Serialize(ret)}");
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
        public ReactiveCommand<OllamaApiMeta, Unit> MockCommand { get; }
        [Reactive]
        public string? ReplyContent
        {
            get;
            set;
        }

        public ObservableCollection<OllamaApiMeta> ApiCollection
        {
            get;
        }
    }
}
