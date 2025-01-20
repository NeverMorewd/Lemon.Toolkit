using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace Lemon.HandyLib.Logging.Definitions
{
    public class LogSettingModel
    {
        [JsonPropertyName("serilog:remain_count")]
        public int RemainFileCount
        {
            get;
            set;
        } = 50;

        [JsonPropertyName("serilog:minimum_level")]
        public LogLevel Level
        {
            get;
            set;
        } = LogLevel.Information;

        [JsonPropertyName("serilog:hold_days")]
        public int RemainDay
        {
            get;
            set;
        } = 7;

        [JsonPropertyName("serilog:root_path")]
        public string RootPath
        {
            get;
            set;
        } = string.Empty;

        public string LevelString
        {
            get
            {
                return Level.ToString();
            }
        }

        public static LogSettingModel Default => new();
    }
}
