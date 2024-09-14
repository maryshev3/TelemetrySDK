using CollectorBase.Extensions;
using CollectorBase.Models;
using CollectorBase.Statistic;
using Newtonsoft.Json;
using TelemetrySetterBase.Models;

namespace Collector.Api.Services;

public class CollectorService
{
    private readonly TelemetryStatistic _telemetryStatistic;
    
    public CollectorService(TelemetryStatistic telemetryStatistic)
    {
        _telemetryStatistic = telemetryStatistic;
    }
    
    private TelemetryItem[] DeserializeAndCreateTreeFromFlat(string telemetriesFlatJson) =>
        FlatToTree(JsonConvert.DeserializeObject<IEnumerable<TelemetryItem>>(telemetriesFlatJson));
    
    public TelemetryItem[] FlatToTree(IFormFile telemetriesFile)
    {
        using Stream stream = telemetriesFile.OpenReadStream();
        using StreamReader streamReader = new StreamReader(stream);

        string telemetriesJson = streamReader.ReadToEnd();

        TelemetryItem[] telemetriesTree = DeserializeAndCreateTreeFromFlat(telemetriesJson);
        
        return telemetriesTree;
    }
    
    public TelemetryItem[] FlatToTree(IEnumerable<TelemetryItem> telemetries) =>
        telemetries.CreateTree().ToArray();

    public async Task<Report> GetStatistics(TelemetryItem[] telemetryTree) =>
        await _telemetryStatistic.CreateReport(telemetryTree);
}