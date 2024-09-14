using System.Diagnostics;

namespace OpenTelemetrySetter.Extensions;

public static class StringExtensions
{
    public static ActivityListener ToActivityListener(
        this string serviceName,
        Action<Activity> activityStoped,
        SampleActivity<ActivityContext> sample
    ) => new ActivityListener()
        {
            ShouldListenTo = activitySource => true,
            ActivityStopped = activityStoped,
            Sample = sample
        };
}