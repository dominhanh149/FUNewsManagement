using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("fe-api/admin/audit-log")]
public class AdminAuditLogProxyController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    public AdminAuditLogProxyController(IHttpClientFactory http) => _http = http;

    [HttpPost]
    public async Task<IActionResult> GetAuditLog([FromBody] object body)
    {
        var client = _http.CreateClient("CoreApi");
        var json = JsonSerializer.Serialize(body);
        var res = await client.PostAsync("/api/NewsArticles/audit-log",
            new StringContent(json, Encoding.UTF8, "application/json"));
        var responseJson = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, responseJson);
    }
}
