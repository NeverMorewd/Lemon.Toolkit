using Lemon.Toolkit.Models.Ollama;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lemon.Toolkit.Models.JsonContexts;

[JsonSerializable(typeof(OllamaGenerateRequest))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<int>))]
public partial class OllamaRequestContext : JsonSerializerContext
{
    
}