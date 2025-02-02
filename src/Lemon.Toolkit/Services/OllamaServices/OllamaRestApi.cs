using Lemon.Toolkit.Models.Ollama;
using Refit;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Lemon.Toolkit.Services.OllamaServices
{
    [Headers("Content-Type: application/json")]
    public interface IOllamaApi
    {
        [Post("/api/generate")]
        Task<OllamaGenerateResponse> GenerateText([Body] OllamaGenerateRequest request);
        
        [Post("/api/chat")]
        Task<OllamaGenerateResponse> Chat([Body] OllamaGenerateRequest request);
        
        [Get("/api/version")]
        Task<JsonObject> GetVersion();
        
        [Get("/api/tags")]
        Task<ModelResponse> ListModels();
        
        [Get("/api/ps")]
        Task<ModelResponse> ListRunningModels();
        
        [Get("/api/show")]
        Task<JsonObject> ShowModel([Body] ModelRequest modelRequest);
        
        [Head("/api/blobs/sha256:{sha256Code}")]
        Task<JsonObject> CheckBlob(string sha256Code);
        
        [Post("/api/blobs/sha256:{sha256Code}")]
        Task<JsonObject> PushBlob(string sha256Code);

        [Post("/api/pull")]
        Task<JsonObject> PullModel([Body] JsonObject request);

        [Delete("/api/delete")]
        Task<JsonObject> DeleteModel([Body] JsonObject request);

        [Get("/api/show")]
        Task<JsonObject> ShowModelInfo([Query] JsonObject request);

        [Get("/health")]
        Task<JsonObject> HealthCheck();
    }
}
