using TelemetrySetterBase.Models;

namespace TelemetrySetterBase.Abstracts;

/// <summary>
/// Интерфейс для сохранения собранной из сервиса телеметрии.
/// </summary>
public interface ISaveManager
{
    /// <summary>
    /// Сохраняет телеметрию.
    /// </summary>
    /// <param name="telemetryItem">Телеметрия</param>
    void Save(TelemetryItem telemetryItem);
}