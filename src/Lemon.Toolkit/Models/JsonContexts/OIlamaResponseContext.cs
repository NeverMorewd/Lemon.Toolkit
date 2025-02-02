using Lemon.Toolkit.Models.Ollama;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lemon.Toolkit.Models.JsonContexts;

[JsonSerializable(typeof(OllamaGenerateResponse))]
[JsonSerializable(typeof(List<int>))]
public partial class OllamaResponseContext:JsonSerializerContext
{
    
}