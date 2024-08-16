namespace CollectorConsole;

public class ActivityStatistic
{
    public string ActivityName { get; set; }
    public TimeSpan AvgDuration { get; set; }
}

public class Statistic
{
    public string SourceName { get; set; }
    public string ActivityName { get; set; }
    public TimeSpan AvgTime { get; set; }
    public TimeSpan AvgDbTime { get; set; }
    public TimeSpan AvgAlgoTime { get; set; }
    public ActivityStatistic[] Top5TimeLessActivities { get; set; }
}