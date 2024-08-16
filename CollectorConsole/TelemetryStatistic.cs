using System.Collections.Concurrent;
using System.Diagnostics;
using CollectorBase.Extensions;
using TelemetrySetterBase.Models;

namespace CollectorConsole;

public static class TelemetryStatistic
{
    private static TimeSpan GetDbTime(TelemetryItem telemetryItem)
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
    
    public static async Task<Statistic[]> CreateStatistic(TelemetryItem[] telemetryItems)
    {
        // Группируем.
        var groups = telemetryItems.GroupBy(x => (x.SourceName, x.ActivityName)).ToArray();

        List<Task<Statistic>> tasks = new();
        
        // Создаём статистику.
        foreach (var group in groups)
        {
            Task<Statistic> task = Task.Run(() =>
                {
                    var activities = group.ToArray();
                    int activitiesCount = activities.Length;
            
                    // Словарь Активность -> Время на работу с БД (в тиках).
                    Dictionary<TelemetryItem, long> telemetryToDbTime = activities
                        .ToDictionary(
                            x => x,
                            x => GetDbTime(x).Ticks
                        );
            
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
                    var top5 = activities
                        .CreateFlat()
                        .Select(x => (x.ActivityDuration - x.ChildrensDuration ?? TimeSpan.Zero, x.ActivityName))
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

                    return new Statistic()
                    {
                        ActivityName = group.Key.ActivityName,
                        SourceName = group.Key.SourceName,
                        AvgAlgoTime = avgAlgoTime,
                        AvgDbTime = avgDbTime,
                        AvgTime = avgTime,
                        Top5TimeLessActivities = top5
                    };
                }
            );
            
            tasks.Add(task);
        }
        
        // Дожидаемся результатов
        return tasks.Select(x => x.Result).ToArray();
    }

    private static void CompareTwoTelemetries(TelemetryItem[] first, TelemetryItem[] second)
    {
        // Группируем.
        var groupedFirst = first.GroupBy(x => (x.SourceName, x.ActivityName)).ToArray();
        var groupedSecond = second.GroupBy(x => (x.SourceName, x.ActivityName)).ToArray();
        
        // 
    }
}