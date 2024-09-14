namespace CollectorBase.Models;

public class Report
{
    public Statistic[] Statistics { get; set; }
    public TimeSpan TotalDbTime { get; set; }
    public TimeSpan TotalAlgoTime { get; set; }
    public TimeSpan TotalTime { get; set; }
}