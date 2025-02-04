using Lemon.Toolkit.Models.Ollama;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Lemon.Toolkit.Services.OllamaServices
{
    public class OllamaFunctionService
    {
        public readonly IOllamaApi _ollamaApi;
        public OllamaFunctionService(IOllamaApi ollamaApi,
            ILogger<OllamaServiceFacade> logger)
        {
            _ollamaApi = ollamaApi;
        }
        public async Task<object> MockApi(string path)
        {
            try
            {
                switch (path)
                {
                    case "/api/generate":
                        return await _ollamaApi.GenerateText(new OllamaGenerateRequest());
                    case "/api/chat":
                        return await _ollamaApi.Chat(new ChatRequest());
                    case "/api/version":
                        return await _ollamaApi.GetVersion();
                    case "/api/tags":
                        return await _ollamaApi.ListModels();
                    case "/api/ps":
                        return await _ollamaApi.ListRunningModels();
                    case "/api/show":
                        return await _ollamaApi.ShowModel(new ModelRequest());
                    case "/api/pull":
                        return await _ollamaApi.PullModel(new JsonObject());
                    case "/api/delete":
                        return await _ollamaApi.DeleteModel(new JsonObject());
                    default:
                        throw new NotSupportedException($"Path {path} is not supported.");
                }
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
