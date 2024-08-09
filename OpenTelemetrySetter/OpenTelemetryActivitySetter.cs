using System.Diagnostics;
using OpenTelemetrySetter.Extensions;
using TelemetrySetterBase;
using TelemetrySetterBase.Abstracts;
using TelemetrySetterBase.Models;

namespace OpenTelemetrySetter;

public class OpenTelemetryActivitySetter : ActivitySetter<Activity>
{
    public OpenTelemetryActivitySetter(
        ISaveManager saveManager,
        IEnumerable<string> servicesForTakeTelemetry,
        IEnumerable<string> tagsToSave
    ) : base(saveManager, servicesForTakeTelemetry, tagsToSave)
    {
    }

    protected override TelemetryItem ToTelemetryItem(Activity telemetry, IEnumerable<string> tagsToSave) => new TelemetryItem()
        {
            Id = telemetry.Id,
            ParentId = telemetry.ParentId,
            SourceName = telemetry.Source.Name,
            ActivityName = telemetry.DisplayName,
            ActivityStart = telemetry.StartTimeUtc,
            ActivityDuration = telemetry.Duration,
            Tags = telemetry.Tags.Where(x => tagsToSave.Contains(x.Key)).ToArray()
        };
    
    // Функции-callback на начало, конец активности.
    private void ActivityEnd(Activity activity) => this.OnActivityEnd(activity);
    private ActivitySamplingResult Sample(ref ActivityCreationOptions<ActivityContext> context) => ActivitySamplingResult.AllData;
    
    public override void Start()
    {
        foreach (var serviceName in _servicesForTakeTelemetry)
        {
            ActivitySource.AddActivityListener(serviceName.ToActivityListener(ActivityEnd, Sample));
        }
    }
}