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

    private static TelemetryItem[] ReadTelemetriesTree(string filePath)
    {
        // Считываем файл телеметрии.
        string telemetriesOriginSerialized = ReadTelemetryFile(filePath);
        
        // Десериализуем плоский список телеметрии.
        IEnumerable<TelemetryItem> telemetriesFlat =
            JsonConvert.DeserializeObject<IEnumerable<TelemetryItem>>(telemetriesOriginSerialized);
        
        // Преобразуем его в древовидный список.
        TelemetryItem[] telemetriesTree = telemetriesFlat.CreateTree().Where(x => x.SourceName != "Npgsql").ToArray();

        return telemetriesTree;
    }
    
    private static void FlatTelemetriesToTreeView()
    {
        TelemetryItem[] telemetriesTree = ReadTelemetriesTree(_originFilePath);
        
        // Сериализуем его.
        string telemetriesTreeSerialized = JsonConvert.SerializeObject(telemetriesTree, Formatting.Indented);
        
        // Сохраняем.
        SaveTelemetryFile(_processedFilePath, telemetriesTreeSerialized);
    }
    
    private static async void CompareTwoTelemetries()
    {
        // Считывание и подготовка данных.
        string firstFilePath = Path.Combine(Environment.CurrentDirectory, "telemetries1.json");
        string secondFilePath = Path.Combine(Environment.CurrentDirectory, "telemetries2.json");
        
        string reportSavePath = Path.Combine(Environment.CurrentDirectory, "compareResult.json");

        var task1 = Task<TelemetryItem[]>.Run(() =>ReadTelemetriesTree(firstFilePath));
        var task2 = Task<TelemetryItem[]>.Run(() =>ReadTelemetriesTree(secondFilePath));

        var firstTelemetriesTree = await task1;
        var secondTelemetriesTree = await task2;
        
        // Сравнение.
        
    }

    private static async Task CreateStatistic()
    {
        TelemetryItem[] telemetriesTree = ReadTelemetriesTree(_originFilePath);
        
        Statistic[] statistics = await TelemetryStatistic.CreateStatistic(telemetriesTree);
        
        // Сериализуем его.
        string statisticsSerialized = JsonConvert.SerializeObject(statistics, Formatting.Indented);
        
        // Сохраняем.
        SaveTelemetryFile("statistics.json", statisticsSerialized);
    }
    
    public static void Main(string[] args)
    {
        // Перевод из плоского списка в древовидный.
        //FlatTelemetriesToTreeView();
        
        // Сравнение двух телеметрий.
        CreateStatistic().Wait();
    }
}