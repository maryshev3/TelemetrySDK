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
        TelemetryItem[] telemetryItemsCopy = telemetryItems.CreateDeepCopy().ToArray();
        
        // Словарь ParentId -> Элементы, у которых одинаковый ParentId.
        var parentIdToItems = telemetryItemsCopy.ToLookup(x => x.ParentId);

        // Заполняем "детей".
        foreach (var item in telemetryItemsCopy)
        {
            item.Childrens = parentIdToItems[item.Id].ToList();
        }
        
        // Возвращаем только корневые элементы.
        // Множество Id.
        var ids = telemetryItemsCopy.Select(x => x.Id).ToHashSet();

        // Список "корневых" элементов.
        return telemetryItemsCopy.Where(x => !ids.Contains(x.ParentId)).ToArray();
    }

    /// <summary>
    /// Создаёт плоский список активностей из древовидного
    /// </summary>
    /// <param name="telemetryItems">Древовидный список телеметрии</param>
    /// <returns>Плоский список телеметрии</returns>
    public static IEnumerable<TelemetryItem> CreateFlat(this IEnumerable<TelemetryItem> telemetryItems)
    {
        List<TelemetryItem> result = new();

        if (!telemetryItems.Any())
        {
            return result;
        }
        
        result.AddRange(telemetryItems);

        result.AddRange(telemetryItems.SelectMany(x => x.Childrens.CreateFlat()));

        return result;
    }
}