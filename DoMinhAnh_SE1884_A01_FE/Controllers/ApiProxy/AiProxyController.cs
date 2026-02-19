using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("fe-api/ai")]
public class AiProxyController : ControllerBase
{
    private readonly IHttpClientFactory _http;

    public AiProxyController(IHttpClientFactory http) => _http = http;

    /// <summary>POST /fe-api/ai/suggest-tags → AI API</summary>
    [HttpPost("suggest-tags")]
    public async Task<IActionResult> SuggestTags([FromBody] object body)
    {
        var client = _http.CreateClient("AiApi");
        var res = await client.PostAsJsonAsync("/api/ai/suggest-tags", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }

    /// <summary>POST /fe-api/ai/learn → AI API (ghi nhận tag user chọn)</summary>
    [HttpPost("learn")]
    public async Task<IActionResult> Learn([FromBody] object body)
    {
        var client = _http.CreateClient("AiApi");
        var res = await client.PostAsJsonAsync("/api/ai/learn", body);
        var json = await res.Content.ReadAsStringAsync();
        return StatusCode((int)res.StatusCode, json);
    }
}
