using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace Lemon.Toolkit.Models.Ollama
{

    public class ModelResponse
    {
        [JsonPropertyName("models")]
        public List<ModelInfo> Models { get; set; }
    }

    public class ModelInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("modified_at")]
        public string ModifiedAt { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("digest")]
        public string Digest { get; set; }

        [JsonPropertyName("details")]
        public ModelDetails Details { get; set; }

        [JsonPropertyName("expires_at")]
        public string ExpiresAt { get; set; }

        [JsonPropertyName("size_vram")]
        public long SizeVram { get; set; }
    }

    public class ModelDetails
    {
        [JsonPropertyName("parent_model")]
        public string ParentModel { get; set; }
        [JsonPropertyName("format")]
        public string Format { get; set; }

        [JsonPropertyName("family")]
        public string Family { get; set; }

        [JsonPropertyName("families")]
        public object Families { get; set; }

        [JsonPropertyName("parameter_size")]
        public string ParameterSize { get; set; }

        [JsonPropertyName("quantization_level")]
        public string QuantizationLevel { get; set; }
    }
}
