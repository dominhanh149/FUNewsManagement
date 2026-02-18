using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _http;

    public AuthController(IHttpClientFactory http) => _http = http;
    public IActionResult Login()
    {
        return View(); // Views/Auth/Login.cshtml
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        var client = _http.CreateClient("CoreApi");

        var body = JsonSerializer.Serialize(new { email, password });
        var res = await client.PostAsync("/api/auth/login",
            new StringContent(body, Encoding.UTF8, "application/json"));

        if (!res.IsSuccessStatusCode)
        {
            TempData["error"] = "Login failed";
            return RedirectToAction("Index", "Home");
        }

        var json = await res.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var access = doc.RootElement.GetProperty("access_token").GetString();
        var refresh = doc.RootElement.GetProperty("refresh_token").GetString();

        HttpContext.Session.SetString("access_token", access!);
        HttpContext.Session.SetString("refresh_token", refresh!);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("access_token");
        HttpContext.Session.Remove("refresh_token");
        return RedirectToAction("Index", "Home");
    }
}
