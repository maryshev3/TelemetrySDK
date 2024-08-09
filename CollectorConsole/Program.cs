using System.Collections;
using CollectorBase.Extensions;
using Newtonsoft.Json;
using TelemetrySetterBase.Models;

namespace CollectorConsole;

class Program
{
    private static string _originFilePath = Path.Combine(Environment.CurrentDirectory, "telemetries.json");
    private static string _processedFilePath = Path.Combine(Environment.CurrentDirectory, "telemetriesTree.json");
    
    private static string ReadTelemetryFile(string filePath) => File.ReadAllText(filePath);

    private static void SaveTelemetryFile(string filePath, string telemetriesSerialized) =>
        File.WriteAllText(filePath, telemetriesSerialized);
    
    public static void Main(string[] args)
    {
        // Считываем файл телеметрии.
        string telemetriesOriginSerialized = ReadTelemetryFile(_originFilePath);
        
        // Десериализуем плоский список телеметрии.
        IEnumerable<TelemetryItem> telemetriesFlat =
            JsonConvert.DeserializeObject<IEnumerable<TelemetryItem>>(telemetriesOriginSerialized);
        
        // Преобразуем его в древовидный список.
        IEnumerable<TelemetryItem> telemetriesTree = telemetriesFlat.CreateTree();
        
        // Сериализуем его.
        string telemetriesTreeSerialized = JsonConvert.SerializeObject(telemetriesTree, Formatting.Indented);
        
        // Сохраняем.
        SaveTelemetryFile(_processedFilePath, telemetriesTreeSerialized);
    }
}