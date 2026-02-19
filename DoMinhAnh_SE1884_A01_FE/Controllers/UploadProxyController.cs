using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DoMinhAnh_SE1884_A01_FE.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class UploadProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public UploadProxyController(IHttpClientFactory factory, IConfiguration config)
        {
            _factory = factory;
            _config = config;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            var client = _factory.CreateClient("CoreApi");

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            // Ensure Content-Type is set, or default to generic binary
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(fileContent, "file", file.FileName);

            var res = await client.PostAsync("/api/Upload/image", content);
            
            if (!res.IsSuccessStatusCode)
            {
               var err = await res.Content.ReadAsStringAsync();
               return StatusCode((int)res.StatusCode, err);
            }

            var json = await res.Content.ReadAsStringAsync();
            
            // Backend returns: { "url": "/uploads/..." }
            // We need to prepend Backend URL so valid absolute URL is returned to client
            var baseUrl = _config["ApiUrls:Core"]?.TrimEnd('/');
            
            try 
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("url", out var urlEl))
                {
                    var relativeUrl = urlEl.GetString();
                    if (!string.IsNullOrEmpty(relativeUrl) && relativeUrl.StartsWith("/"))
                    {
                        var absoluteUrl = $"{baseUrl}{relativeUrl}";
                        return Ok(new { url = absoluteUrl });
                    }
                }
            }
            catch {}

            // Fallback: return original json
            return Content(json, "application/json");
        }
    }
}
