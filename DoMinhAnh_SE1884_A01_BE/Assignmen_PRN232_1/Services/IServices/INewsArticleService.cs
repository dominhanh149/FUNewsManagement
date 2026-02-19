using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Dto.Common;
using Assignmen_PRN232_1.DTOs.Common;

namespace Assignmen_PRN232_1.Services.IServices
{
    public interface INewsArticleService
    {
        Task<PagingDto<NewsArticleDto>> GetListPagingAsync(NewsArticleSearchDto dto);
        Task<PagingDto<NewsArticleDto>> GetPublicListPagingAsync(NewsArticleSearchDto dto);
        Task<IEnumerable<NewsArticleDto>> GetAllAsync();
        Task<NewsArticleDto?> GetByIdAsync(string id);

        Task<ApiResponse<NewsArticleDto>> CreateOrEditAsync(NewsArticleSaveDto dto);
        Task<ApiResponse<NewsArticleDto>> DuplicateAsync(string id);
        Task<ApiResponse<bool>> DeleteAsync(string id);

        Task<ApiResponse<bool>> AddTagAsync(string newsArticleId, int tagId);
        Task<ApiResponse<bool>> RemoveTagAsync(string newsArticleId, int tagId);
        Task<ApiResponse<bool>> SetTagsAsync(string newsArticleId, List<int> tagIds);
        Task<bool> IncreaseViewAsync(string id);
        Task<PagingDto<NewsArticleDto>> GetAuditLogAsync(AuditLogSearchDto dto);

    }
}