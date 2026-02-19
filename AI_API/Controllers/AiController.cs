using AI_API.Models;
using AI_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AI_API.Controllers
{
    [ApiController]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly ITagExtractorService _extractor;
        private readonly LearningCacheService _cache;
        private readonly ILogger<AiController> _logger;

        public AiController(
            ITagExtractorService extractor,
            LearningCacheService cache,
            ILogger<AiController> logger)
        {
            _extractor = extractor;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// POST /api/ai/suggest-tags
        /// Input : { "content": "Nội dung bài viết..." }
        /// Output: { "tags": [{ "tag": "...", "confidence": 0.9 }, ...] }
        /// </summary>
        [HttpPost("suggest-tags")]
        public IActionResult SuggestTags([FromBody] SuggestTagsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Content))
                return BadRequest(new { success = false, message = "Content is required." });

            _logger.LogInformation("suggest-tags called, content length={len}", request.Content.Length);

            var tags = _extractor.ExtractTags(request.Content, topN: 10);

            var response = new SuggestTagsResponse
            {
                Tags = tags,
                FromCache = tags.Any(t => _cache.GetBoost(t.Tag) > 1.0)
            };

            return Ok(new { success = true, data = response });
        }

        /// <summary>
        /// POST /api/ai/learn
        /// Ghi nhận các tag user đã chọn để cải thiện gợi ý sau này.
        /// Input: { "selectedTags": ["Tag1", "Tag2"] }
        /// </summary>
        [HttpPost("learn")]
        public IActionResult Learn([FromBody] LearnTagsRequest request)
        {
            if (request?.SelectedTags == null || !request.SelectedTags.Any())
                return BadRequest(new { success = false, message = "selectedTags is required." });

            _cache.RecordSelection(request.SelectedTags);

            _logger.LogInformation("learn: recorded {count} tags: {tags}",
                request.SelectedTags.Count,
                string.Join(", ", request.SelectedTags));

            return Ok(new { success = true, message = $"Learned {request.SelectedTags.Count} tag(s)." });
        }

        /// <summary>
        /// GET /api/ai/cache — xem Learning Cache hiện tại (để debug)
        /// </summary>
        [HttpGet("cache")]
        public IActionResult GetCache()
        {
            var all = _cache.GetAll()
                            .OrderByDescending(kv => kv.Value)
                            .Select(kv => new { tag = kv.Key, selectedCount = kv.Value })
                            .ToList();

            return Ok(new { success = true, data = all, total = all.Count });
        }
    }
}
