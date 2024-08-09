using System.Reflection;
using Newtonsoft.Json;

namespace FileSaver;

public class FileSaveManagerBuilder
{
    private readonly string _settingsPath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..",  "./fileSaveManagerSettings.json");
    
    private Settings ReadSettings(string settingsPath) => JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsPath));
    
    public FileSaveManager Build() => new FileSaveManager(Path.Combine(Assembly.GetExecutingAssembly().Location, "..",  ReadSettings(_settingsPath).SavePath));
}