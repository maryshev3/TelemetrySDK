namespace TelemetrySetterBase.Models;

public class TelemetryItem : ICloneable
{
    public string Id { get; set; }
    public string ParentId { get; set; }
    public string SourceName { get; set; }
    public string ActivityName { get; set; }
    public DateTime ActivityStart { get; set; }
    public TimeSpan ActivityDuration { get; set; }
    public KeyValuePair<string, string>[] Tags { get; set; }
    public List<TelemetryItem> Childrens { get; set; } = new();
    public TimeSpan? ChildrensDuration => !Childrens.Any()
        ? null
        : TimeSpan.FromMilliseconds(Childrens.Sum(x => x.ActivityDuration.TotalMilliseconds));

    public object Clone() => new TelemetryItem()
        {
            Id = this.Id,
            ParentId = this.ParentId,
            SourceName = this.SourceName,
            ActivityName = this.ActivityName,
            ActivityStart = this.ActivityStart,
            ActivityDuration = this.ActivityDuration,
            Tags = this.Tags.Select(x => x).ToArray(),
            Childrens = Childrens.Any()
                ? Childrens.Select(x => x.Clone() as TelemetryItem).ToList()
                : Childrens.Select(x => x).ToList()
        };
}