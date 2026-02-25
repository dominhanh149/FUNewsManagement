using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Background Worker: Tự động gọi các API endpoint quan trọng để làm mới dữ liệu
/// cache phía server (IMemoryCache) mỗi 6 giờ (configurable).
/// 
/// Cách hoạt động:
/// - Khởi động cùng app (IHostedService / BackgroundService)
/// - Sau mỗi interval, gọi HttpClient tới các endpoint cần cache
/// - Log ra console để dễ quan sát khi test
/// 
/// Cách test nhanh: Đổi RefreshIntervalSeconds = 30 trong appsettings.Development.json
/// </summary>
public class CacheRefreshWorker : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<CacheRefreshWorker> _logger;

    // Các route FE-proxy sẽ "warm up" / refresh
    private static readonly string[] EndpointsToRefresh =
    [
        "/fe-api/staff/news/public/paging",   // public news list
        "/fe-api/Category",                    // categories
        "/fe-api/Tag",                         // tags
    ];

    public CacheRefreshWorker(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<CacheRefreshWorker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Lấy interval từ config (mặc định 6 giờ = 21600 giây)
        // Khi test: đặt "CacheRefresh:IntervalSeconds": 30 trong appsettings.Development.json
        var intervalSeconds = _config.GetValue<int>("CacheRefresh:IntervalSeconds", 21600);
        var interval = TimeSpan.FromSeconds(intervalSeconds);

        _logger.LogInformation(
            "⏰ CacheRefreshWorker started. Interval: {Interval} ({Seconds}s)",
            interval, intervalSeconds);

        // Chờ một chút sau khi app khởi động
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("🔄 CacheRefreshWorker: Starting cache refresh at {Time}", DateTimeOffset.Now);
            await RefreshAllCachesAsync(stoppingToken);
            _logger.LogInformation("✅ CacheRefreshWorker: Cache refresh completed. Next run in {Interval}", interval);

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task RefreshAllCachesAsync(CancellationToken cancellationToken)
    {
        // Dùng client nội bộ gọi lại chính FE app (localhost)
        // để các proxy controller chạy lại → backend API được gọi → cache được làm mới
        var client = _httpClientFactory.CreateClient("SelfRefresh");

        foreach (var endpoint in EndpointsToRefresh)
        {
            try
            {
                _logger.LogInformation("  → Refreshing: {Endpoint}", endpoint);

                HttpResponseMessage res;
                if (endpoint.Contains("/paging"))
                {
                    // POST endpoints cần body hợp lệ tránh 400 Bad Request
                    var body = new { 
                        page = 1, 
                        pageSize = 20, 
                        isActive = true, // Tùy thuộc vào model backend
                        keyword = "" 
                    };
                    res = await client.PostAsJsonAsync(endpoint, body, cancellationToken);
                }
                else
                {
                    res = await client.GetAsync(endpoint, cancellationToken);
                }

                _logger.LogInformation("  ✓ {Endpoint} → {Status}", endpoint, res.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("  ✗ {Endpoint} failed: {Error}", endpoint, ex.Message);
            }
        }
    }
}
