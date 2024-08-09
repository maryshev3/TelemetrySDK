using System.Collections;
using TelemetrySetterBase.Models;

namespace CollectorBase.Extensions;

public static class EnumerableEx
{
    public static IEnumerable<TelemetryItem> CreateDeepCopy(this IEnumerable<TelemetryItem> telemetryItems) =>
        telemetryItems.Select(x => x.Clone() as TelemetryItem);
    
    /// <summary>
    /// Из плоского списка телеметрии формирует древовидный список телеметрии.
    /// </summary>
    /// <param name="telemetryItems">Плоский список телеметрии</param>
    /// <returns>Древовидный список телеметрии</returns>
    public static IEnumerable<TelemetryItem> CreateTree(this IEnumerable<TelemetryItem> telemetryItems)
    {
        IEnumerable<TelemetryItem> telemetryItemsCopy = telemetryItems.CreateDeepCopy();

        (Dictionary<string, List<string>> idToChildrens, Dictionary<string, TelemetryItem> idToTelemetryItems, HashSet<string> childrenIds) mapsOfTelemetries = telemetryItemsCopy
            .Aggregate(
                (new Dictionary<string, List<string>>(), new Dictionary<string, TelemetryItem>(), new HashSet<string>()),
                (maps, telemetryItem) =>
                {
                    // Пополняем словарь ид -> ид детей
                    if (!maps.Item1.ContainsKey(telemetryItem.Id))
                    {
                        maps.Item1.Add(telemetryItem.Id, new List<string>());
                    }
                    
                    if (maps.Item1.ContainsKey(telemetryItem.ParentId))
                    {
                        maps.Item1[telemetryItem.ParentId].Add(telemetryItem.Id);
                        
                        // Это не корневой элемент. Добавляем его в список "детей".
                        maps.Item3.Add(telemetryItem.Id);
                    }
                    else
                    {
                        maps.Item1.Add(telemetryItem.ParentId, new List<string>());
                    }
                    
                    // Пополняем словарь ид -> объект телеметрии
                    maps.Item2.Add(telemetryItem.Id, telemetryItem);
                    
                    return (maps.Item1, maps.Item2, maps.Item3);
                }
            );

        return mapsOfTelemetries
            .idToChildrens
            .Where(x => 
                mapsOfTelemetries.idToTelemetryItems.ContainsKey(x.Key)
                && !mapsOfTelemetries.childrenIds.Contains(x.Key)
            )
            .Select(x =>
            {
                TelemetryItem parent = mapsOfTelemetries.idToTelemetryItems[x.Key];

                parent.Childrens = x.Value.Select(y => mapsOfTelemetries.idToTelemetryItems[y]).ToList();

                return parent;
            }
        );
    }
}