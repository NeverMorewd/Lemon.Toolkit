using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lemon.HandyLib.Logging.Definitions;

namespace Lemon.HandyLib.Logging
{
    public static class LogSettingsHelper
    {
        private readonly static JsonSerializerOptions _options;
        static LogSettingsHelper()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public static string LogConfigFilePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogSettings.json");

        public static LogSettingModel CurrentSettings
        {
            get;
            set;
        } = LoadSettings();

        public static void Refresh()
        {
            CurrentSettings = LoadSettings();
        }

        private static LogSettingModel LoadSettings()
        {
            try
            {
                if (File.Exists(LogConfigFilePath))
                {
                    using FileStream fileStream = File.Open(LogConfigFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    using StreamReader sw = new(fileStream);
                    string json = sw.ReadToEnd();
                    return JsonSerializer.Deserialize<LogSettingModel>(json)!;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fail to load config file:{ex}");
            }
            SaveSettings(LogSettingModel.Default);

            return LogSettingModel.Default;
        }

        public static void SaveSettings(LogSettingModel logSettingModel)
        {
            try
            {
                logSettingModel ??= LogSettingModel.Default;
                var json = JsonSerializer.Serialize(logSettingModel, _options);
                File.WriteAllText(LogConfigFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ail to save config file:{ex}");
            }
        }
    }
}