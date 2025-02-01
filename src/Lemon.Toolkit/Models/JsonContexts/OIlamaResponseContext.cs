using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lemon.Toolkit.Models.JsonContexts;

[JsonSerializable(typeof(OllamaResponse))]
[JsonSerializable(typeof(List<int>))]
public partial class OllamaResponseContext:JsonSerializerContext
{
    
}