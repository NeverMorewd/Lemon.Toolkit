using System;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Lemon.Toolkit.Models;

namespace Lemon.Toolkit.Services.OllamaServices
{
    public class OllamaServiceFacade
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly IOllamaApi _ollamaApi;
        private const string CurrentModelName = "qwen2:7b";

        private readonly string test =
            "(Invoke-WebRequest -method POST -Body '{\"model\":\"llama3.2\", \"prompt\":\"Why is the sky blue?\", \"stream\": false}' -uri http://localhost:11434/api/generate ).Content | ConvertFrom-json";

        public OllamaServiceFacade(IHttpClientFactory httpClientFactory, 
            IOllamaApi ollamaApi,  
            ILogger<OllamaServiceFacade> logger)
        {
            _logger = logger;
            _ollamaApi = ollamaApi;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
        }
        public async Task<string> Ask(string content)
        {
            try
            {
                var prompt = new OllamaGenerateRequest(CurrentModelName, content, stream: false);
                var response = await _ollamaApi.GenerateText(prompt);
                return response.Response!;

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ask error");
                return exception.Message;
            }
        }
        public async Task<string> AskOriginal(string content)
        {
            try
            {
                var prompt = new OllamaGenerateRequest(CurrentModelName, content, stream: false);
                var response = await _httpClient.PostAsJsonAsync("http://localhost:11434/api/generate", prompt);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return $"{response.StatusCode}:{response.Content}";

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ask error");
                return exception.Message;
            }
        }
    }
}
