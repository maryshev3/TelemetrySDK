using TelemetrySetterBase.Abstracts;
using TelemetrySetterBase.Models;
using Newtonsoft.Json;

namespace FileSaver;

public class FileSaveManager : ISaveManager
{
    private readonly string _savePath;

    private readonly object _locker = new();

    public FileSaveManager(string savePath)
    {
        _savePath = savePath;
        
        CreateFileIfNotExist(savePath);
    }

    private void CreateFileIfNotExist(string savePath)
    {
        if (!File.Exists(savePath))
            File.WriteAllText(savePath, "[]");
    }
    
    private string GetExistedTelemetries() => File.ReadAllText(_savePath);
    private void SaveTelemetries(string telemetriesSerialized) => File.WriteAllText(_savePath, telemetriesSerialized);
    
    public void Save(TelemetryItem telemetryItem)
    {
        lock (_locker)
        {
            List<TelemetryItem> existedTelemetryItems =
                JsonConvert.DeserializeObject<List<TelemetryItem>>(GetExistedTelemetries());
            
            existedTelemetryItems.Add(telemetryItem);
            
            string serializedTelemetryItems = JsonConvert.SerializeObject(existedTelemetryItems);
            
            SaveTelemetries(serializedTelemetryItems);
        }
    }
}