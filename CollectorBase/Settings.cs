using System.Reflection;
using Newtonsoft.Json;

namespace CollectorBase;

public class StatisticSettings
{
    public HashSet<string> SourcesBlackList { get; set; }
}

public class Settings
{
    private static string _settingsPath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..", "collectorSettings.json");
    
    public StatisticSettings Statistic { get; set; }

    public static Settings Init() => JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_settingsPath));
}