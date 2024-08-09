using System.Reflection;
using Newtonsoft.Json;

namespace TelemetrySetterBase;

public class Settings
{
    private readonly string _settingsPath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..",  "./telemetrySetterSettings.json");
    
    public IEnumerable<string> ServicesForTakeTelemetry { get; set; }
    public IEnumerable<string> TagsToSave { get; set; }

    /// <summary>
    /// Создаёт экземпляр настроек из конфигурации.
    /// </summary>
    /// <returns></returns>
    public Settings Init() => JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_settingsPath));
}