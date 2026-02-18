using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("fe-api/admin/accounts")]
public class AdminAccountsProxyController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    public AdminAccountsProxyController(IHttpClientFactory http) => _http = http;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Dictionary<string, string> query)
    {
        var client = _http.CreateClient("CoreApi");
        var qs = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var res = await client.GetAsync("/api/SystemAccounts" + (qs.Length > 0 ? "?" + qs : ""));
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(short id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.GetAsync($"/api/SystemAccounts/{id}");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpPost("create-or-edit")]
    public async Task<IActionResult> CreateOrEdit([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.PostAsJsonAsync("/api/SystemAccounts/create-or-edit", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(short id)
    {
        var client = _http.CreateClient("CoreApi");
        var res = await client.DeleteAsync($"/api/SystemAccounts/{id}");
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }
}
