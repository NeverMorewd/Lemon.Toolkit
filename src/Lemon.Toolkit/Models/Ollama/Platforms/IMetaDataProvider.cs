using System.Collections.Generic;
using System.Collections.Specialized;

namespace Lemon.Toolkit.Models.Ollama.Platforms
{
    public interface IMetaDataProvider
    {
        public StringDictionary? EnvironmentVariables 
        { 
            get;
            set;
        }
        public string ProcessName_App
        {
            get;
        }
        public string ProcessName_Core
        {
            get;
        }
        public string ProcessName_Server
        {
            get;
        }
        public string AppFileName
        {
            get;
        }
        public string ServerLogFileName
        {
            get;
        }
        public string ServerLogFilePath
        {
            get;
        }
        public string LogDirectory
        {
            get;
        }
    }
}
