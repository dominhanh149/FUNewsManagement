using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("fe-api/auth")]
public class AuthProxyController : ControllerBase
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<AuthProxyController> _logger;

    public AuthProxyController(IHttpClientFactory http, ILogger<AuthProxyController> logger)
    {
        _http = http;
        _logger = logger;
    }

    public class LoginReq
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginReq req)
    {
        var client = _http.CreateClient("CoreApi");

        var body = JsonSerializer.Serialize(new
        {
            AccountEmail = req.Email,
            AccountPassword = req.Password
        });

        var res = await client.PostAsync("/api/SystemAccounts/login",
            new StringContent(body, Encoding.UTF8, "application/json"));

        var json = await res.Content.ReadAsStringAsync();

        if (!res.IsSuccessStatusCode)
            return StatusCode((int)res.StatusCode, json);

        using var doc = JsonDocument.Parse(json);

        // Get values from backend response
        var accessToken = doc.RootElement.GetProperty("access_token").GetString();
        var refreshToken = doc.RootElement.GetProperty("refresh_token").GetString();
        var role = doc.RootElement.GetProperty("role").GetString();

        // Store in session
        HttpContext.Session.SetString("access_token", accessToken!);
        HttpContext.Session.SetString("refresh_token", refreshToken!);

        // Simple user object
        var user = new
        {
            accountEmail = req.Email,
            role = role
        };

        return Ok(new { success = true, user, token = accessToken });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("access_token");
        return Ok(new { success = true });
    }
}