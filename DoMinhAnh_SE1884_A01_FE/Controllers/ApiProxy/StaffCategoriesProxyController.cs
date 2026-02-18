using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("fe-api/Category")]
public class StaffCategoriesProxyController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    public StaffCategoriesProxyController(IHttpClientFactory http) => _http = http;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Dictionary<string, string> query)
    {
        var client = _http.CreateClient("CoreApi");
        var qs = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var res = await client.GetAsync("/api/Category" + (qs.Length > 0 ? "?" + qs : ""));
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsJsonAsync("/api/Category/search", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.GetAsync($"/api/Category/{id}");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("{id:int}/article-count")]
    public async Task<IActionResult> GetArticleCount(int id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.GetAsync($"/api/Category/{id}/article-count");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsJsonAsync("/api/Category", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PutAsJsonAsync($"/api/Category/{id}", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPost("create-or-edit")]
    public async Task<IActionResult> CreateOrEdit([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsJsonAsync("/api/Category/create-or-edit", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPatch("toggle-status")]
    public async Task<IActionResult> ToggleStatus([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PatchAsJsonAsync("/api/Category/toggle-status", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.DeleteAsync($"/api/Category/{id}");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }
}
