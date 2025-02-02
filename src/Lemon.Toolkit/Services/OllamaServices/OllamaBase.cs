namespace Lemon.Toolkit.Services.OllamaServices
{
    public static class OllamaBase
    {
        static OllamaBase() { }
        public static string[] Models => ["qwen2:7b", 
            "phi3:3.8b", 
            "gemma2:9b", 
            "llama3.2:8b", 
            "deepseek-r1:8b"];


    }
}
