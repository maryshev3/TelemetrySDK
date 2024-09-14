namespace CollectorBase.Models;

public class Statistic
{
    public string SourceName { get; set; }
    public string ActivityName { get; set; }
    public TimeSpan AvgTime { get; set; }
    public TimeSpan AvgDbTime { get; set; }
    public TimeSpan AvgAlgoTime { get; set; }
    public TimeSpan TotalTime { get; set; }
    public TimeSpan TotalDbTime { get; set; }
    public TimeSpan TotalAlgoTime { get; set; }
    public ActivityStatistic[] Top5TimeLessActivities { get; set; }
}