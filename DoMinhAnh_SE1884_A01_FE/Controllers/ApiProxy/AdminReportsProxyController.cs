using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("fe-api/Reports")]
public class AdminReportsProxyController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    public AdminReportsProxyController(IHttpClientFactory http) => _http = http;

    [HttpPost("news-articles")]
    public async Task<IActionResult> NewsArticles([FromBody] JsonElement body)
    {
        var client = _http.CreateClient("CoreApi");
        var content = new StringContent(body.GetRawText(), Encoding.UTF8, "application/json");
        var res = await client.PostAsync("/api/reports/news-articles", content);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] Dictionary<string, string> query)
    {
        var client = _http.CreateClient("AnalyticsApi");
        var qs = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var res = await client.GetAsync("/api/analytics/dashboard" + (qs.Length > 0 ? "?" + qs : ""));
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("trending")]
    public async Task<IActionResult> Trending()
    {
        var client = _http.CreateClient("AnalyticsApi");
        var res = await client.GetAsync("/api/analytics/trending");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] Dictionary<string, string> query)
    {
        var client = _http.CreateClient("AnalyticsApi");
        var qs = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var res = await client.GetAsync("/api/analytics/export" + (qs.Length > 0 ? "?" + qs : ""));

        var bytes = await res.Content.ReadAsByteArrayAsync();
        var contentType = res.Content.Headers.ContentType?.ToString()
                         ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return File(bytes, contentType, "report.xlsx");
    }
}

