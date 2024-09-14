using System.Diagnostics;
using OpenTelemetrySetter.Extensions;
using TelemetrySetterBase;
using TelemetrySetterBase.Abstracts;
using TelemetrySetterBase.Models;

namespace OpenTelemetrySetter;

public class OpenTelemetryActivitySetter : ActivitySetter<Activity>
{
    public OpenTelemetryActivitySetter(
        ISaveManager saveManager
    ) : base(saveManager)
    {
    }

    protected override TelemetryItem ToTelemetryItem(Activity telemetry) => new TelemetryItem()
        {
            Id = telemetry.Id,
            ParentId = telemetry.ParentId,
            SourceName = telemetry.Source.Name,
            ActivityName = telemetry.DisplayName,
            ActivityStart = telemetry.StartTimeUtc,
            ActivityDuration = telemetry.Duration,
            Tags = telemetry.Tags.ToArray()
        };
    
    // Функции-callback на начало, конец активности.
    private void ActivityEnd(Activity activity) => this.OnActivityEnd(activity);
    private ActivitySamplingResult Sample(ref ActivityCreationOptions<ActivityContext> context) => ActivitySamplingResult.AllData;
    
    public override void Start() =>
        ActivitySource.AddActivityListener(new ActivityListener()
            {
                ShouldListenTo = activitySource => true,
                ActivityStopped = ActivityEnd,
                Sample = Sample
            }
        );
}