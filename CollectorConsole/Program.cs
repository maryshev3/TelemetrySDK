using System.Collections;
using System.Text;
using CollectorBase;
using CollectorBase.Extensions;
using CollectorBase.Models;
using CollectorBase.Statistic;
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
        TelemetryItem[] telemetriesTree = telemetriesFlat.CreateTree().Where(x => x.SourceName != "Npgsql"/* && x.ActivityName == "MergeAsync"*/).ToArray();

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

        // DateTime start = new DateTime(2024, 8, 19, 10, 41, 0, DateTimeKind.Utc);
        // DateTime end = new DateTime(2024, 8, 19, 17, 54, 0, DateTimeKind.Utc);
        
        // DateTime start = new DateTime(2024, 8, 16, 7, 11, 9, DateTimeKind.Utc);
        // DateTime end = new DateTime(2024, 8, 16, 9, 47, 28, DateTimeKind.Utc);

        // TelemetryItem[] telemetryItemsFiltered =
        //     telemetriesTree.Where(x => x.ActivityStart >= start && x.ActivityStart <= end).ToArray();

        TelemetryItem[] telemetryItemsFiltered =
            telemetriesTree;

        var settings = Settings.Init();

        var telemetryStatistic = new TelemetryStatistic(settings);
        
        Report report = await telemetryStatistic.CreateReport(telemetryItemsFiltered);
        
        // Сериализуем его.
        string statisticsSerialized = JsonConvert.SerializeObject(report, Formatting.Indented);
        
        // Сохраняем.
        SaveTelemetryFile("statistics.json", statisticsSerialized);
    }

    private static void StatisticToCsv(string statPath, string savePath)
    {
        Report report = JsonConvert.DeserializeObject<Report>(ReadTelemetryFile(statPath));

        StringBuilder stringBuilder = new();

        foreach (var statistic in report.Statistics)
        {
            stringBuilder.AppendLine($"{statistic.ActivityName};{statistic.SourceName};{statistic.AvgTime};{statistic.AvgDbTime};{statistic.AvgAlgoTime};{statistic.TotalTime};{statistic.TotalDbTime};{statistic.TotalAlgoTime};");
        }
        
        File.WriteAllText(savePath, stringBuilder.ToString());
    }

    private static void CompareStatisticsInCsv(Report first, Report second)
    {
        var comparedStatistics = first
            .Statistics
            .Join(
                second.Statistics,
                x => (x.SourceName, x.ActivityName),
                x => (x.SourceName, x.ActivityName),
                (firstStat, secondStat) =>
                {
                    return new
                    {
                        ActivityName = firstStat.ActivityName,
                        SourceName = firstStat.SourceName,
                        TotalTime = secondStat.TotalTime / firstStat.TotalTime,
                        TotalDbTime = secondStat.TotalDbTime / firstStat.TotalDbTime,
                        TotalAlgoTime = secondStat.TotalAlgoTime / firstStat.TotalAlgoTime,
                    };
                    
                    /*return new
                    {
                        ActivityName = firstStat.ActivityName,
                        SourceName = firstStat.SourceName,
                        TotalTime = (double)(secondStat.TotalTime.Ticks - firstStat.TotalTime.Ticks) / firstStat.TotalTime.Ticks * 100,
                        TotalDbTime = (double)(secondStat.TotalDbTime.Ticks - firstStat.TotalDbTime.Ticks) / firstStat.TotalDbTime.Ticks * 100,
                        TotalAlgoTime = (double)(secondStat.TotalAlgoTime.Ticks - firstStat.TotalAlgoTime.Ticks) / firstStat.TotalAlgoTime.Ticks * 100,
                    };*/
                }
            )
            .ToArray();
        
        StringBuilder stringBuilder = new();

        foreach (var statistic in comparedStatistics)
        {
            stringBuilder.AppendLine($"{statistic.ActivityName};{statistic.SourceName};{statistic.TotalTime};{statistic.TotalDbTime};{statistic.TotalAlgoTime};");
        }
        
        string savePath = Path.Combine(Environment.CurrentDirectory, "report_compare.csv");
        
        File.WriteAllText(savePath, stringBuilder.ToString());
    }

    private static void CompareStatistics()
    {
        string dev3StatPath = Path.Combine(Environment.CurrentDirectory, "report_dev3.json");
        string tmpStatPath = Path.Combine(Environment.CurrentDirectory, "report_tmp.json");
        
        Report dev3Report = JsonConvert.DeserializeObject<Report>(ReadTelemetryFile(dev3StatPath));
        Report tmpReport = JsonConvert.DeserializeObject<Report>(ReadTelemetryFile(tmpStatPath));
        
        CompareStatisticsInCsv(dev3Report, tmpReport);
    }
    
    private static void StatisticsToCsv()
    {
        string dev3StatPath = Path.Combine(Environment.CurrentDirectory, "report_dev3.json");
        string tmpStatPath = Path.Combine(Environment.CurrentDirectory, "report_tmp.json");
        
        string dev3SavePath = Path.Combine(Environment.CurrentDirectory, "report_dev3.csv");
        string tmpSavePath = Path.Combine(Environment.CurrentDirectory, "report_tmp.csv");
        
        StatisticToCsv(dev3StatPath, dev3SavePath);
        StatisticToCsv(tmpStatPath, tmpSavePath);
    }
    
    public static void Main(string[] args)
    {
        // Перевод из плоского списка в древовидный.
        FlatTelemetriesToTreeView();
        
        // Создание статистики на основе телеметрии.
        //CreateStatistic().Wait();
        
        // Статистика на основе телеметрии -> csv формат.
        //StatisticsToCsv();
        
        // Сравнение статистик -> csv формат.
        //CompareStatistics();
    }
}