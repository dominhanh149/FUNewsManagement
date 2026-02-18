using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Dto.Common;
using Assignmen_PRN232_1.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Assignmen_PRN232_1.Controllers.Api
{
    //[Authorize(Roles = "Staff, Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly ILogger<TagController> _logger;

        public TagController(ITagService tagService, ILogger<TagController> logger)
        {
            _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region CRUD Operations

        /// <summary>
        /// Get all tags
        /// GET: api/tag
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TagDto>>>> GetAllTags()
        {
            try
            {
                var tags = await _tagService.GetAllTagsAsync();
                return Ok(ApiResponse<IEnumerable<TagDto>>.SuccessResponse(tags, "Tags retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all tags.");
                return StatusCode(500, ApiResponse<IEnumerable<TagDto>>.ErrorResponse(
                    "An error occurred while retrieving tags.", ex.Message));
            }
        }

        /// <summary>
        /// Get tag by ID
        /// GET: api/tag/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<TagDto>>> GetTagById(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);
                if (tag == null)
                {
                    return NotFound(ApiResponse<TagDto>.ErrorResponse($"Tag with ID {id} not found."));
                }

                return Ok(ApiResponse<TagDto>.SuccessResponse(tag, "Tag retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting tag {TagId}.", id);
                return StatusCode(500, ApiResponse<TagDto>.ErrorResponse(
                    "An error occurred while retrieving the tag.", ex.Message));
            }
        }

        /// <summary>
        /// Get tag with all related articles
        /// GET: api/tag/{id}/articles
        /// </summary>
        [HttpGet("{id}/articles")]
        public async Task<ActionResult<ApiResponse<TagWithArticlesDto>>> GetTagWithArticles(int id)
        {
            try
            {
                var tag = await _tagService.GetTagWithArticlesAsync(id);
                if (tag == null)
                {
                    return NotFound(ApiResponse<TagWithArticlesDto>.ErrorResponse($"Tag with ID {id} not found."));
                }

                return Ok(ApiResponse<TagWithArticlesDto>.SuccessResponse(tag, "Tag with articles retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting tag {TagId} with articles.", id);
                return StatusCode(500, ApiResponse<TagWithArticlesDto>.ErrorResponse(
                    "An error occurred while retrieving the tag with articles.", ex.Message));
            }
        }

        /// <summary>
        /// Create a new tag
        /// POST: api/tag
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TagDto>>> CreateTag([FromBody] CreateTagDto createTagDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse<TagDto>.ErrorResponse("Validation failed.", errors));
                }

                var tag = await _tagService.CreateTagAsync(createTagDto);
                return CreatedAtAction(
                    nameof(GetTagById),
                    new { id = tag.TagID },
                    ApiResponse<TagDto>.SuccessResponse(tag, "Tag created successfully."));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while creating tag.");
                return BadRequest(ApiResponse<TagDto>.ErrorResponse("Failed to create tag.", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating tag.");
                return StatusCode(500, ApiResponse<TagDto>.ErrorResponse(
                    "An error occurred while creating the tag.", ex.Message));
            }
        }

        /// <summary>
        /// Update an existing tag
        /// PUT: api/tag/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse>> UpdateTag(int id, [FromBody] UpdateTagDto updateTagDto)
        {
            try
            {
                if (id != updateTagDto.TagID)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Tag ID mismatch."));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(ApiResponse.ErrorResponse("Validation failed.", errors));
                }

                var result = await _tagService.UpdateTagAsync(updateTagDto);
                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse($"Tag with ID {id} not found."));
                }

                return Ok(ApiResponse.SuccessResponse("Tag updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tag {TagId} not found for update.", id);
                return NotFound(ApiResponse.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating tag {TagId}.", id);
                return BadRequest(ApiResponse.ErrorResponse("Failed to update tag.", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating tag {TagId}.", id);
                return StatusCode(500, ApiResponse.ErrorResponse(
                    "An error occurred while updating the tag.", ex.Message));
            }
        }

        /// <summary>
        /// Delete a tag
        /// DELETE: api/tag/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> DeleteTag(int id)
        {
            try
            {
                var result = await _tagService.DeleteTagAsync(id);
                if (!result)
                {
                    return NotFound(ApiResponse.ErrorResponse($"Tag with ID {id} not found."));
                }

                return Ok(ApiResponse.SuccessResponse("Tag deleted successfully."));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot delete tag {TagId}.", id);
                return BadRequest(ApiResponse.ErrorResponse("Failed to delete tag.", ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting tag {TagId}.", id);
                return StatusCode(500, ApiResponse.ErrorResponse(
                    "An error occurred while deleting the tag.", ex.Message));
            }
        }

        #endregion

        #region Search and Filter

        /// <summary>
        /// Search tags with pagination and filtering
        /// POST: api/tag/search
        /// </summary>
        [HttpPost("search")]
        public async Task<ActionResult<ApiResponse<PaginatedTagResponse>>> SearchTags([FromBody] TagSearchDto searchDto)
        {
            try
            {
                var result = await _tagService.SearchTagsAsync(searchDto);
                return Ok(ApiResponse<PaginatedTagResponse>.SuccessResponse(result, "Tags search completed successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching tags.");
                return StatusCode(500, ApiResponse<PaginatedTagResponse>.ErrorResponse(
                    "An error occurred while searching tags.", ex.Message));
            }
        }

        /// <summary>
        /// Get articles by tag ID
        /// GET: api/tag/{id}/articles-list
        /// </summary>
        [HttpGet("{id}/articles-list")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TagArticleDto>>>> GetArticlesByTag(int id)
        {
            try
            {
                var articles = await _tagService.GetArticlesByTagAsync(id);
                return Ok(ApiResponse<IEnumerable<TagArticleDto>>.SuccessResponse(
                    articles, $"Articles for tag {id} retrieved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Tag {TagId} not found.", id);
                return NotFound(ApiResponse<IEnumerable<TagArticleDto>>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting articles for tag {TagId}.", id);
                return StatusCode(500, ApiResponse<IEnumerable<TagArticleDto>>.ErrorResponse(
                    "An error occurred while retrieving articles.", ex.Message));
            }
        }

        /// <summary>
        /// Get tags by article ID
        /// GET: api/tag/by-article/{articleId}
        /// </summary>
        [HttpGet("by-article/{articleId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TagDto>>>> GetTagsByArticle(string articleId)
        {
            try
            {
                var tags = await _tagService.GetTagsByArticleIdAsync(articleId);
                return Ok(ApiResponse<IEnumerable<TagDto>>.SuccessResponse(
                    tags, $"Tags for article {articleId} retrieved successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid article ID: {ArticleId}", articleId);
                return BadRequest(ApiResponse<IEnumerable<TagDto>>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting tags for article {ArticleId}.", articleId);
                return StatusCode(500, ApiResponse<IEnumerable<TagDto>>.ErrorResponse(
                    "An error occurred while retrieving tags.", ex.Message));
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Check if tag exists
        /// GET: api/tag/{id}/exists
        /// </summary>
        [HttpGet("{id}/exists")]
        public async Task<ActionResult<ApiResponse<bool>>> TagExists(int id)
        {
            try
            {
                var exists = await _tagService.TagExistsAsync(id);
                return Ok(ApiResponse<bool>.SuccessResponse(exists));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if tag {TagId} exists.", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "An error occurred while checking tag existence.", ex.Message));
            }
        }

        /// <summary>
        /// Check if tag name is available
        /// GET: api/tag/name-available?name={name}&excludeId={excludeId}
        /// </summary>
        [HttpGet("name-available")]
        public async Task<ActionResult<ApiResponse<bool>>> IsTagNameAvailable([FromQuery] string name, [FromQuery] int? excludeId = null)
        {
            try
            {
                var available = await _tagService.IsTagNameAvailableAsync(name, excludeId);
                return Ok(ApiResponse<bool>.SuccessResponse(available));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid tag name provided.");
                return BadRequest(ApiResponse<bool>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking tag name availability.");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse(
                    "An error occurred while checking tag name availability.", ex.Message));
            }
        }

        /// <summary>
        /// Check if tag can be deleted
        /// GET: api/tag/{id}/can-delete
        /// </summary>
        #endregion
    }
}