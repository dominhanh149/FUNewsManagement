using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("fe-api/Tag")]
public class StaffTagsProxyController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    public StaffTagsProxyController(IHttpClientFactory http) => _http = http;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Dictionary<string, string> query)
    {
        var client = _http.CreateClient("CoreApi");
        var qs = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var res = await client.GetAsync("/api/Tag" + (qs.Length > 0 ? "?" + qs : ""));
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsJsonAsync("/api/Tag/search", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.GetAsync($"/api/Tag/{id}");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("{id:int}/articles-list")]
    public async Task<IActionResult> GetArticlesList(int id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.GetAsync($"/api/Tag/{id}/articles-list");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    /// <summary>POST /fe-api/Tag – tạo tag mới</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsJsonAsync("/api/Tag", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    /// <summary>PUT /fe-api/Tag/{id} – cập nhật tag</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PutAsJsonAsync($"/api/Tag/{id}", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPost("create-or-edit")]
    public async Task<IActionResult> CreateOrEdit([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsJsonAsync("/api/Tag/create-or-edit", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.DeleteAsync($"/api/Tag/{id}");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }
}
