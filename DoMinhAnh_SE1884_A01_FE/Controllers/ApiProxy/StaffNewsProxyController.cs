using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("fe-api/staff/news")]
public class StaffNewsProxyController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    public StaffNewsProxyController(IHttpClientFactory http) => _http = http;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Dictionary<string, string> query)
    {
        var client = _http.CreateClient("CoreApi");
        var qs = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var res = await client.GetAsync("/api/NewsArticles" + (qs.Length > 0 ? "?" + qs : ""));
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.GetAsync($"/api/NewsArticles/{id}");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPost("paging")]
    public async Task<IActionResult> GetPaging([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsJsonAsync("/api/NewsArticles/paging", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPost("public/paging")]
    public async Task<IActionResult> GetPublicPaging([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsJsonAsync("/api/NewsArticles/public/paging", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPost("create-or-edit")]
    public async Task<IActionResult> CreateOrEdit([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsJsonAsync("/api/NewsArticles/create-or-edit", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPost("{id}/duplicate")]
    public async Task<IActionResult> Duplicate(string id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsync($"/api/NewsArticles/{id}/duplicate", null);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.DeleteAsync($"/api/NewsArticles/{id}");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    /// <summary>POST /fe-api/staff/news/{id}/view — tăng ViewCount</summary>
    [HttpPost("{id}/view")]
    public async Task<IActionResult> IncreaseView(string id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsync($"/api/NewsArticles/{id}/view", null);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    /// <summary>GET /fe-api/staff/news/recommend/{id} — gợi ý bài liên quan</summary>
    [HttpGet("recommend/{id}")]
    public async Task<IActionResult> Recommend(string id)
    {
        var client = _http.CreateClient("AnalyticsApi");
        var res = await client.GetAsync($"/api/recommend/{id}");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }
}
