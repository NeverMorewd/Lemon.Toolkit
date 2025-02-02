using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Lemon.Toolkit.Models.Ollama;

public struct OllamaGenerateRequest
{
    public OllamaGenerateRequest(
        string? model,
        string? prompt,
        List<string>? images = null,
        string? format = null,
        Dictionary<string, object>? options = null,
        string? system = null,
        string? template = null,
        List<int>? context = null,
        bool stream = false,
        bool raw = false,
        string? keepAlive = null)
    {
        Model = model;
        Prompt = prompt;
        Images = images;
        Format = format;
        Options = options;
        System = system;
        Template = template;
        Context = context;
        Stream = stream;
        Raw = raw;
        KeepAlive = keepAlive;
    }
    [JsonPropertyName("model")]
    public string? Model { get; private set; }

    [JsonPropertyName("prompt")]
    public string? Prompt { get; private set; }

    [JsonPropertyName("images")]
    public List<string>? Images { get; private set; }

    [JsonPropertyName("format")]
    public string? Format { get; private set; }

    [JsonPropertyName("options")]
    public Dictionary<string, object>? Options { get; private set; }

    [JsonPropertyName("system")]
    public string? System { get; private set; }

    [JsonPropertyName("template")]
    public string? Template { get; private set; }

    [JsonPropertyName("context")]
    public List<int>? Context { get; private set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; private set; }

    [JsonPropertyName("raw")]
    public bool Raw { get; private set; }

    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; private set; }

}