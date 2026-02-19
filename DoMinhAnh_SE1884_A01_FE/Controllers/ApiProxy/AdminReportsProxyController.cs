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
    public async Task<IActionResult> Dashboard()
    {
        var client = _http.CreateClient("AnalyticsApi");
        // Forward toàn bộ query string từ FE sang Analytics API
        var qs = Request.QueryString.Value ?? "";
        var res = await client.GetAsync("/api/analytics/dashboard" + qs);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("trending")]
    public async Task<IActionResult> Trending()
    {
        var client = _http.CreateClient("AnalyticsApi");
        var qs = Request.QueryString.Value ?? "";
        var res = await client.GetAsync("/api/analytics/trending" + qs);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export()
    {
        var client = _http.CreateClient("AnalyticsApi");
        var qs = Request.QueryString.Value ?? "";
        var res = await client.GetAsync("/api/analytics/export" + qs);

        var bytes = await res.Content.ReadAsByteArrayAsync();
        var contentType = res.Content.Headers.ContentType?.ToString()
                         ?? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        // Lấy filename từ header nếu có
        var disposition = res.Content.Headers.ContentDisposition?.FileNameStar
                          ?? res.Content.Headers.ContentDisposition?.FileName
                          ?? $"FUNews_Report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        disposition = disposition.Trim('"');

        return File(bytes, contentType, disposition);
    }
}
