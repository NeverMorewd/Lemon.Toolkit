using Lemon.Toolkit.Models.Ollama;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Lemon.Toolkit.Services.OllamaServices
{
    public class OllamaServiceFacade
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly IOllamaApi _ollamaApi;
        private const string CurrentModelName = "qwen2:7b";
        private readonly OllamaFunctionService _ollamaFunctionService;
        private readonly OllamaManageService _ollamaManageService;
        private readonly IEnumerable<IChatClient> _chatClients;

        private readonly string test =
            "(Invoke-WebRequest -method POST -Body '{\"model\":\"llama3.2\", \"prompt\":\"Why is the sky blue?\", \"stream\": false}' -uri http://localhost:11434/api/generate ).Content | ConvertFrom-json";

        public OllamaServiceFacade(IHttpClientFactory httpClientFactory, 
            IOllamaApi ollamaApi,
            OllamaFunctionService ollamaFunctionService,
            OllamaManageService ollamaManageService,
            IEnumerable<IChatClient> chatClients,
            ILogger<OllamaServiceFacade> logger)
        {
            _logger = logger;
            _ollamaApi = ollamaApi;
            _chatClients = chatClients;
            _ollamaManageService = ollamaManageService;
            _ollamaFunctionService = ollamaFunctionService;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
        }
        private string? currentModel;
        public string? CurrentModel
        {
            get
            {
                return currentModel;
            }
            set
            {
                currentModel = value;
            }
        }
        public async Task<string> Ask(string content)
        {
            try
            {
                var prompt = new OllamaGenerateRequest(CurrentModel, content, stream: false);
                var response = await _ollamaApi.GenerateText(prompt);
                return response.Response!;

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ask error");
                return exception.Message;
            }
        }
        public async Task<string> Chat(string content)
        {
            try
            {
                var prompt = new ChatRequest
                {
                    Stream = false,
                    Messages = [ new Message 
                    {
                        Role = "user",
                        Content = content
                    }],
                    Model = CurrentModel!,
                };
                var response = await _ollamaApi.Chat(prompt);
                return response.Message.Content!;

            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Ask error");
                return exception.Message;
            }
        }
        public async Task LoadModel(string modelName)
        {
            try
            {
                var prompt = new OllamaGenerateRequest(CurrentModel);
                var response = await _ollamaApi.GenerateText(prompt);
                if (!response.Done)
                {
                    throw new Exception($"Fail to load {modelName}");
                }
            }
            catch(Exception ex)
            { }
        }
        public async Task UnloadModel(string modelName)
        {
            try
            {
                var prompt = new OllamaGenerateRequest(CurrentModel, keepAlive: 0);
                var response = await _ollamaApi.GenerateText(prompt);
                if (!response.Done)
                {
                    throw new Exception($"Fail to Unload {modelName}");
                }
            }
            catch(Exception ex)
            { }
        }
        public async Task<string> AskOriginal(string content)
        {
            try
            {
                var prompt = new OllamaGenerateRequest(CurrentModel, content, stream: false);
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

        public async Task<object> Mock(string path)
        {
            return await _ollamaFunctionService.MockApi(path);
        }

        public async Task<string?> GetPath()
        {
            return await _ollamaManageService.SearchPathAsync();
        }
        public async Task<IEnumerable<string>> GetAvailableModels()
        {
            return (await GetModels()).Where(m=>!IsEmbed(m));
        }
        public async Task<IEnumerable<string>> GetModels()
        {
            return await _ollamaManageService.GetModels();
        }
        public bool IsEmbed(string modelName)
        {
            return modelName.Contains("-embed-");
        }
    }
}
