using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace TmsApi.Api.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
public class SearchController : ControllerBase
{
    [HttpGet("search")]
    [EnableRateLimiting("search")]
    public IActionResult SearchCourses([FromQuery] string? term)
    {
        return Ok(new
        {
            term = term ?? "all",
            results = new[] { "CSE-101", "CSE-201", "CSE-301" }
        });
    }
}