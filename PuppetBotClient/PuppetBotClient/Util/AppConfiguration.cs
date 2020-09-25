using PuppetBotClient.Util.AppConfigModels;
using System.IO;
using System.Text.Json;

namespace PuppetBotClient.Util
{
    public static partial class AppConfiguration
    {
        public const string AppSettingsFileName = "appsettings.json";
        public static AppSettingsJsonModel Settings { get; private set; }

        static AppConfiguration()
        {
            var jsonAppSettingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), AppSettingsFileName);
            var rawJsonAppSettings = File.ReadAllText(jsonAppSettingsFilePath);
            var jsonConfig = JsonSerializer.Deserialize<AppSettingsJsonModel>(rawJsonAppSettings);

            Settings = jsonConfig;
        }
    }
}
