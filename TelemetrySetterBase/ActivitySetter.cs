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
    protected readonly IEnumerable<string> _servicesForTakeTelemetry;
    protected readonly IEnumerable<string> _tagsToSave;

    protected ActivitySetter(ISaveManager saveManager, IEnumerable<string> servicesForTakeTelemetry, IEnumerable<string> tagsToSave)
    {
        _saveManager = saveManager;
        _servicesForTakeTelemetry = servicesForTakeTelemetry;
        _tagsToSave = tagsToSave;
    }

    protected abstract TelemetryItem ToTelemetryItem(T telemetry, IEnumerable<string> tagsToSave);

    protected void OnActivityEnd(T telemetry)
    {
        TelemetryItem converted = this.ToTelemetryItem(telemetry, _tagsToSave);
        
        _saveManager.Save(converted);
    }

    /// <summary>
    /// Начинает слушать все активности и сохранять их по завершении.
    /// Тут стоит регистрировать обработчики событий "Начало активности" и "Конец активности".
    /// </summary>
    public abstract void Start();
}