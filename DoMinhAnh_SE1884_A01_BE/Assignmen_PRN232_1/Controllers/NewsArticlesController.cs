using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Dto.Common;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232_1.Hubs;
using Assignmen_PRN232_1.Services;
using Assignmen_PRN232_1.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Assignmen_PRN232_1.Controllers
{
    [Authorize(Roles = "Staff, Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class NewsArticlesController : ControllerBase
    {
        private readonly INewsArticleService _service;
        private readonly AppDbContext _db;
        private readonly IHubContext<NewsHub> _hub;

        public NewsArticlesController(INewsArticleService service, AppDbContext db, IHubContext<NewsHub> hub)
        {
            _service = service;
            _db = db;
            _hub = hub;
        }
        [HttpGet]
        public async Task<IActionResult> GetListPaging([FromQuery] NewsArticleSearchDto dto)
        {
            var result = await _service.GetListPagingAsync(dto);
            return Ok(result);
        }

        [HttpPost("paging")]
        public async Task<IActionResult> GetPaging([FromBody] NewsArticleSearchDto dto)
        {
            var result = await _service.GetListPagingAsync(dto);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        [HttpPost("public/paging")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicPaging([FromBody] NewsArticleSearchDto dto)
        {
            var result = await _service.GetPublicListPagingAsync(dto);
            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound(ApiResponse.ErrorResponse("NewsArticle not found."));
            return Ok(ApiResponse<object>.SuccessResponse(item));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NewsArticleSaveDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            dto.CreatedById = GetCurrentUserId();

            var result = await _service.CreateOrEditAsync(dto);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// POST /api/NewsArticles/create-or-edit
        /// Tạo mới nếu NewsArticleId rỗng, cập nhật nếu có ID.
        /// </summary>
        [HttpPost("create-or-edit")]
        public async Task<IActionResult> CreateOrEdit([FromBody] NewsArticleSaveDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isNew = string.IsNullOrWhiteSpace(dto.NewsArticleId);

            if (isNew)
                dto.CreatedById = GetCurrentUserId();
            else
            {
                dto.UpdatedById = GetCurrentUserId();
                dto.ModifiedDate = DateTime.UtcNow;
            }

            var result = await _service.CreateOrEditAsync(dto);

            if (!result.Success)
            {
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result);
                return BadRequest(result);
            }

            // SignalR broadcast
            try
            {
                var action = isNew ? "created" : "updated";
                var article = result.Data;
                if (article != null && article.NewsStatus == true)
                {
                    await _hub.Clients.Group("public").SendAsync("ReceiveArticleUpdate", new Assignmen_PRN232_1.Hubs.ArticleNotification
                    {
                        Action = action,
                        NewsArticleId = article.NewsArticleId,
                        NewsTitle = article.NewsTitle,
                        CategoryName = article.CategoryName,
                        AuthorName = article.CreatedByName,
                        NewsStatus = article.NewsStatus
                    });
                }
            }
            catch { /* ignore signalr errors */ }

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] NewsArticleSaveDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!string.Equals(id, dto.NewsArticleId, StringComparison.OrdinalIgnoreCase))
                return BadRequest(ApiResponse.ErrorResponse("NewsArticle ID mismatch."));

            dto.UpdatedById = GetCurrentUserId();
            dto.ModifiedDate = DateTime.UtcNow;

            var result = await _service.CreateOrEditAsync(dto);

            if (!result.Success)
            {
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result);

                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result.Success)
            {
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result);

                return BadRequest(result);
            }

            // SignalR broadcast
            try
            {
                await _hub.Clients.Group("public").SendAsync("ReceiveArticleUpdate", new Assignmen_PRN232_1.Hubs.ArticleNotification
                {
                    Action = "deleted",
                    NewsArticleId = id
                });
            }
            catch { }

            return Ok(result);
        }

        [HttpPost("{id}/duplicate")]
        public async Task<IActionResult> Duplicate(string id)
        {
            var result = await _service.DuplicateAsync(id);
            if (!result.Success)
            {
                if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
                    return NotFound(result);

                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/tags/{tagId:int}")]
        public async Task<IActionResult> AddTag(string id, int tagId)
        {
            var result = await _service.AddTagAsync(id, tagId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}/tags/{tagId:int}")]
        public async Task<IActionResult> RemoveTag(string id, int tagId)
        {
            var result = await _service.RemoveTagAsync(id, tagId);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        // set many tags one request
        [HttpPut("{id}/tags")]
        public async Task<IActionResult> SetTags(string id, [FromBody] List<int> tagIds)
        {
            var result = await _service.SetTagsAsync(id, tagIds);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        private short GetCurrentUserId()
        {
            var val =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("id") ??
                User.FindFirstValue("accountId");

            if (string.IsNullOrWhiteSpace(val))
                throw new UnauthorizedAccessException("Missing user id claim.");

            if (!short.TryParse(val, out var id))
                throw new UnauthorizedAccessException("Invalid user id claim type.");

            return id;
        }
        [HttpPost("{id}/view")]
        [AllowAnonymous]
        public async Task<IActionResult> IncreaseView(string id)
        {
            var success = await _service.IncreaseViewAsync(id);

            if (!success)
                return NotFound();

            // Notify view count update check
            var article = await _db.NewsArticles.FindAsync(id);
            if (article != null)
            {
                 await _hub.Clients.Group("public").SendAsync("ReceiveViewUpdate", new Assignmen_PRN232_1.Hubs.ViewCountNotification
                 {
                     NewsArticleId = id,
                     ViewCount = article.ViewCount
                 });
            }

            return Ok(new { success = true });
        }

        /// <summary>
        /// GET audit log: all articles with editor info. Admin only.
        /// </summary>
        [HttpPost("audit-log")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuditLog([FromBody] AuditLogSearchDto dto)
        {
            var result = await _service.GetAuditLogAsync(dto);
            return Ok(result);
        }

    }
}
