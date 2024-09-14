using CollectorBase.Extensions;
using CollectorBase.Models;
using TelemetrySetterBase.Models;

namespace CollectorBase.Statistic;

public class TelemetryStatistic
{
    private readonly Settings _settings;

    public TelemetryStatistic(Settings settings)
    {
        _settings = settings;
    }
    
    private TimeSpan GetDbTime(TelemetryItem telemetryItem)
    {
        if (telemetryItem.SourceName == "Npgsql")
            return telemetryItem.ActivityDuration;

        TimeSpan accumulator = TimeSpan.Zero;
        
        foreach (var child in telemetryItem.Childrens)
        {
            accumulator += GetDbTime(child);
        }

        return accumulator;
    }

    private TelemetryItem[] PreprocessTelemetries(TelemetryItem[] telemetryItems) =>
        telemetryItems
            .CreateFlat()
            .Where(x => !_settings.Statistic.SourcesBlackList.Contains(x.SourceName))
            .CreateTree()
            .ToArray();
    
    public async Task<Report> CreateReport(TelemetryItem[] telemetryItems)
    {
        // Отсеиваем ненужные активности.
        telemetryItems = PreprocessTelemetries(telemetryItems);
        
        // Группируем.
        var groups = telemetryItems.GroupBy(x => (x.SourceName, x.ActivityName)).ToArray();

        List<Task<Models.Statistic>> tasks = new();
        
        // Создаём статистику.
        foreach (var group in groups)
        {
            Task<Models.Statistic> task = Task.Run(() =>
                {
                    var activities = group.ToArray();
                    int activitiesCount = activities.Length;
            
                    // Словарь Активность -> Время на работу с БД (в тиках).
                    Dictionary<TelemetryItem, long> telemetryToDbTime = activities
                        .ToDictionary(
                            x => x,
                            x => GetDbTime(x).Ticks
                        );
            
                    // Общее время работы на алгоритмику и запросы к БД.
                    TimeSpan thisTotalTime, thisTotalDbTime, thisTotalAlgoTime;
                    thisTotalTime = thisTotalDbTime = thisTotalAlgoTime = TimeSpan.Zero;
                    foreach (var item in activities)
                    {
                        var thisDbTime = new TimeSpan(telemetryToDbTime[item]);
            
                        thisTotalTime += item.ActivityDuration;
                        thisTotalDbTime += thisDbTime;
                        thisTotalAlgoTime += item.ActivityDuration - thisDbTime;
                    }
                    
                    
                    // Среднее время на работу с БД.
                    TimeSpan avgDbTime = new TimeSpan(activities.Select(x => (int)(telemetryToDbTime[x] / activitiesCount)).Sum());
            
                    // Среднее время на выполнение корневой активности, не связанной с БД.
                    TimeSpan avgAlgoTime = new TimeSpan(activities.Select(x => (int)((x.ActivityDuration.Ticks - telemetryToDbTime[x]) / activitiesCount)).Sum());
            
                    // Среднее время активности с учётом подактивностей и БД.
                    TimeSpan avgTime =
                        new TimeSpan(activities.Select(x => (int)(x.ActivityDuration.Ticks / activitiesCount)).Sum());
                    
                    // Составляем топ 5 самых длительных алгоритмически активностей.
                    // Из времени алгоритмической активности исключено время выполнения подактивностей.
                    // Т.е только время разных обработок считается.
                    // БД запросы исключены отсюда.

                    if (group.Key.SourceName.Contains("Substation"))
                        ;
                    
                    var top5 = activities
                        .CreateFlat()
                        .Select(x => (x.ActivityDuration - (x.ChildrensDuration ?? TimeSpan.Zero), x.ActivityName))
                        .ToArray()
                        .GroupBy(x => x.ActivityName)
                        .Select(x => new ActivityStatistic()
                            {
                                ActivityName = x.Key,
                                AvgDuration = new TimeSpan(0, 0, 0, 0, (int)x.Select(x => x.Item1.TotalMilliseconds).Average())
                            }
                        )
                        .OrderByDescending(x => x.AvgDuration)
                        .Take(5)
                        .ToArray();

                    var childs = activities
                        .CreateFlat()
                        .Select(x => (x.ActivityDuration - x.ChildrensDuration ?? TimeSpan.Zero, x.ActivityName))
                        .ToArray();

                    return new Models.Statistic()
                    {
                        ActivityName = group.Key.ActivityName,
                        SourceName = group.Key.SourceName,
                        AvgAlgoTime = avgAlgoTime,
                        AvgDbTime = avgDbTime,
                        AvgTime = avgTime,
                        TotalTime = thisTotalTime,
                        TotalDbTime = thisTotalDbTime,
                        TotalAlgoTime = thisTotalAlgoTime,
                        Top5TimeLessActivities = top5
                    };
                }
            );
            
            tasks.Add(task);
        }
        
        // Формируем общее время, затраченное на разные вещи по всей интеграции.
        TimeSpan totalTime, totalDbTime, totalAlgoTime;
        totalTime = totalDbTime = totalAlgoTime = TimeSpan.Zero;
        
        foreach (var item in telemetryItems)
        {
            var thisDbTime = GetDbTime(item);
            
            totalTime += item.ActivityDuration;
            totalDbTime += thisDbTime;
            totalAlgoTime += item.ActivityDuration - thisDbTime;
        }
        
        // Создаём отчёт.
        Report report = new Report()
        {
            Statistics = tasks.Select(x => x.Result).ToArray(),
            TotalTime = totalTime,
            TotalAlgoTime = totalAlgoTime,
            TotalDbTime = totalDbTime
        };
        
        return report;
    }

    private static void CompareTwoTelemetries(TelemetryItem[] first, TelemetryItem[] second)
    {
        // Группируем.
        var groupedFirst = first.GroupBy(x => (x.SourceName, x.ActivityName)).ToArray();
        var groupedSecond = second.GroupBy(x => (x.SourceName, x.ActivityName)).ToArray();
        
        // 
    }
}