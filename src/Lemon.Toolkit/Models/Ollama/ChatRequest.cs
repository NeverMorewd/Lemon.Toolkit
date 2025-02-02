using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Lemon.Toolkit.Models.Ollama
{

    public struct ChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        /// If messages is epmty array, ollama will load model
        /// </summary>
        [JsonPropertyName("messages")]
        public List<Message> Messages { get; set; }

        [JsonPropertyName("tools")]
        public List<object> Tools { get; set; }

        [JsonPropertyName("format")]
        public Format Format { get; set; }

        /// <summary>
        /// https://github.com/ollama/ollama/blob/main/docs/modelfile.md#valid-parameters-and-values
        /// </summary>
        [JsonPropertyName("options")]
        public Dictionary<string, object> Options { get; set; }

        [JsonPropertyName("stream")]
        public bool? Stream { get; set; }

        /// <summary>
        /// set keepalive is 0 and messages is emtpy can unload a model
        /// </summary>
        [JsonPropertyName("keep_alive")]
        public int KeepAlive { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("images")]
        public List<string> Images { get; set; }

        [JsonPropertyName("tool_calls")]
        public List<object> ToolCalls { get; set; }
    }

    public class Format
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("required")]
        public string[] Required { get; set; }
        [JsonPropertyName("properties")]
        public Dictionary<string,FormatProperty> Properties { get; set; }
    }

    public struct FormatProperty
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

    public class Options
    {
        public const string OPTION_SEED = "seed";
        public const string OPTION_TEMPERATURE = "temperature";

        public readonly string[] DoneReasons = ["stop","load","unload"];

    }

    public class FunctionCallInfo
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public FunctionInfo Function { get; set; }
    }

    public class FunctionInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("parameters")]
        public ParameterInfo Parameters { get; set; }
    }

    public class ParameterInfo
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, PropertyInfo> Properties { get; set; }

        [JsonPropertyName("required")]
        public List<string> Required { get; set; }
    }

    public class PropertyInfo
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("enum")]
        public List<string> EnumValues { get; set; }
    }

    #region response
    public class FunctionCall
    {
        [JsonPropertyName("function")]
        public Function Function { get; set; }
    }

    public class Function
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("arguments")]
        public Dictionary<string,object> Arguments { get; set; }
    }
    #endregion
}
