using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Toolkit.Models.Ollama.Platforms
{
    public class MetaDataWindows : IMetaDataProvider
    {
        public MetaDataWindows() 
        {

        }
        public StringDictionary? EnvironmentVariables 
        { 
            get; 
            set; 
        }
        public string ProcessName_App => "ollama app";
        public string ProcessName_Core => "ollama";
        public string ProcessName_Server => "ollama_llama_server";
        public string AppFileName => "ollama app.exe";
        public string ServerLogFileName => "server.log";
        public string ServerLogFilePath => Path.Combine(LogDirectory, ServerLogFileName);
        public string LogDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Ollama");
    }
}
