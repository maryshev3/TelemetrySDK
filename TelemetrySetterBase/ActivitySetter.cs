using TelemetrySetterBase.Abstracts;
using TelemetrySetterBase.Models;

namespace TelemetrySetterBase;

/// <summary>
/// Класс, фиксирующий активность, её завершение, сохранение.
/// </summary>
/// <typeparam name="T">Конкретная реализация телеметрии</typeparam>
public abstract class ActivitySetter<T> where T: class
{
    protected readonly ISaveManager _saveManager;

    protected ActivitySetter(ISaveManager saveManager)
    {
        _saveManager = saveManager;
    }

    protected abstract TelemetryItem ToTelemetryItem(T telemetry);

    protected void OnActivityEnd(T telemetry)
    {
        TelemetryItem converted = this.ToTelemetryItem(telemetry);
        
        _saveManager.Save(converted);
    }

    /// <summary>
    /// Начинает слушать все активности и сохранять их по завершении.
    /// Тут стоит регистрировать обработчики событий "Начало активности" и "Конец активности".
    /// </summary>
    public abstract void Start();
}