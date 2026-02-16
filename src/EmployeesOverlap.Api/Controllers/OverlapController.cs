using EmployeesOverlap.Core;
using Microsoft.AspNetCore.Mvc;

namespace EmployeesOverlap.Api.Controllers;

public sealed class AnalyzeRequest
{
    public IFormFile File { get; set; } = default!;
}

[ApiController]
[Route("api/[controller]")]
public sealed class OverlapController : ControllerBase
{
    [HttpPost("analyze")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)] // 50 MB
    public ActionResult<PairResult> Analyze([FromForm] AnalyzeRequest request)
    {
        var file = request.File;

        if (file == null || file.Length == 0)
            return BadRequest("No file was uploaded.");

        if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Please upload a .csv file.");

        var tmpPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");

        try
        {
            using (var fs = System.IO.File.Create(tmpPath))
                file.CopyTo(fs);

            var records = CsvLoader.Load(tmpPath);
            var result = OverlapCalculator.FindBestPair(records);
            return Ok(result);
        }
        catch (FormatException ex)
        {
            return BadRequest(ex.Message);
        }
        finally
        {
            try { System.IO.File.Delete(tmpPath); } catch { }
        }
    }
}
