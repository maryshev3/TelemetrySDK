using System.Reflection;
using FileSaver;
using TelemetrySetterManager.Configuration;
using Newtonsoft.Json;
using OpenTelemetrySetter;
using TelemetrySetterBase;
using TelemetrySetterBase.Abstracts;
using Settings = TelemetrySetterManager.Configuration.Settings;

namespace TelemetrySetterManager;

public static class Startup
{
    private static string _settingsPath = Path.Combine(Assembly.GetExecutingAssembly().Location, "..",  "./settings.json");
    
    /// <summary>
    /// Метод считывает настройки из соответствующего файла.
    /// </summary>
    /// <param name="settingsPath">Путь к файлу настроек</param>
    /// <returns>Настройки работы сборщика телеметрии</returns>
    private static Settings ReadSettings(string settingsPath) => JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsPath));

    /// <summary>
    /// Возвращает экземпляр менеджера для сохранения телеметрии.
    /// </summary>
    /// <param name="saverType">Тип менеджера для сохранения телеметрии</param>
    /// <returns>Менеджер для сохранения телеметрии</returns>
    private static ISaveManager GetSaver(SaverType saverType) => saverType switch
    {
        SaverType.FileSaver => new FileSaveManagerBuilder().Build()
    };

    private static void RunTelemetrySetter(TelemetrySetterType telemetrySetterType, ISaveManager saveManager)
    {
        // Считываем настройки сеттера телеметрии.
        TelemetrySetterBase.Settings settings = new TelemetrySetterBase.Settings().Init();
        
        // Запускаем сбор телеметрии.
        switch (telemetrySetterType)
        {
            case TelemetrySetterType.OpenTelemetrySetter:
                new OpenTelemetryActivitySetter(saveManager, settings.ServicesForTakeTelemetry, settings.TagsToSave).Start();
                break;
        }
    }

    /// <summary>
    /// Метод начинает слушать телеметрию на основе реализации СеттераТелеметрии и СейвераТелеметрии 
    /// </summary>
    public static void Main()
    {
        // Считываем настройки сбора телеметрии.
        Settings settings = ReadSettings(_settingsPath);
        
        // Получаем менеджер для сохранения телеметрии.
        ISaveManager saveManager = GetSaver(settings.SaverType);
        
        // Запускаем сбор телеметрии.
        RunTelemetrySetter(settings.TelemetrySetterType, saveManager);
    }
}