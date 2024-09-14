using Microsoft.AspNetCore.Mvc;
using System.IO;
using Collector.Api.Services;
using CollectorBase.Extensions;
using CollectorBase.Models;
using Newtonsoft.Json;
using TelemetrySetterBase.Models;

namespace Collector.Api.Controllers;

public class CollectorController : ControllerBase
{
    private readonly CollectorService _collectorService;
    
    public CollectorController(CollectorService collectorService)
    {
        _collectorService = collectorService;
    }
    
    [HttpPost("flat-to-tree-from-file")]
    [ProducesResponseType(typeof(TelemetryItem[]), StatusCodes.Status200OK)]
    public IActionResult FlatToTree(IFormFile telemetriesFile)
    {
        try
        {
            TelemetryItem[] telemetriesTree = _collectorService.FlatToTree(telemetriesFile);
        
            return Ok(telemetriesTree);
        }
        catch (Exception exception)
        {
            ModelState.AddModelError("Error", exception.ToString());

            return BadRequest(ModelState);
        }
    }
    
    [HttpPost("flat-to-tree")]
    [ProducesResponseType(typeof(TelemetryItem[]), StatusCodes.Status200OK)]
    public IActionResult FlatToTree([FromBody] TelemetryItem[] telemetries)
    {
        try
        {
            TelemetryItem[] telemetriesTree = _collectorService.FlatToTree(telemetries);
        
            return Ok(telemetriesTree);
        }
        catch (Exception exception)
        {
            ModelState.AddModelError("Error", exception.ToString());

            return BadRequest(ModelState);
        }
    }
    
    [HttpPost("get-statistics")]
    [ProducesResponseType(typeof(TelemetryItem[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics([FromBody] TelemetryItem[] telemetriesTree)
    {
        try
        {
            Report report = await _collectorService.GetStatistics(telemetriesTree);
        
            return Ok(report);
        }
        catch (Exception exception)
        {
            ModelState.AddModelError("Error", exception.ToString());

            return BadRequest(ModelState);
        }
    }
}