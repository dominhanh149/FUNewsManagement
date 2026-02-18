using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class ApiAuthHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public ApiAuthHandler(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx == null) return await base.SendAsync(request, ct);

        var accessToken = ctx.Session.GetString("access_token");
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        var response = await base.SendAsync(request, ct);

        // Nếu token hết hạn -> refresh rồi retry 1 lần
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshed = await TryRefreshTokenAsync(ctx, ct);
            if (!refreshed) return response;

            // clone request để gửi lại (HttpRequestMessage chỉ gửi 1 lần)
            var retryRequest = await CloneHttpRequestMessageAsync(request);
            var newAccess = ctx.Session.GetString("access_token");
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccess);

            response.Dispose();
            return await base.SendAsync(retryRequest, ct);
        }

        return response;
    }

    private async Task<bool> TryRefreshTokenAsync(HttpContext ctx, CancellationToken ct)
    {
        var refreshToken = ctx.Session.GetString("refresh_token");
        if (string.IsNullOrWhiteSpace(refreshToken)) return false;

        // gọi Core API refresh
        var core = _httpClientFactory.CreateClient("CoreApi");

        var payload = JsonSerializer.Serialize(new { refreshToken });
        var res = await core.PostAsync("/api/auth/refresh",
            new StringContent(payload, Encoding.UTF8, "application/json"), ct);

        if (!res.IsSuccessStatusCode) return false;

        var json = await res.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        // map theo response bạn trả
        var access = doc.RootElement.GetProperty("access_token").GetString();
        var refresh = doc.RootElement.GetProperty("refresh_token").GetString();

        if (string.IsNullOrWhiteSpace(access) || string.IsNullOrWhiteSpace(refresh)) return false;

        ctx.Session.SetString("access_token", access);
        ctx.Session.SetString("refresh_token", refresh);

        return true;
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        // headers
        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        // content
        if (request.Content != null)
        {
            var ms = new MemoryStream();
            await request.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
