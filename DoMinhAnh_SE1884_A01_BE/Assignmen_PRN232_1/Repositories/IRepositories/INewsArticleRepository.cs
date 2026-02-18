using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232_1.DTOs.Common;

namespace Assignmen_PRN232__.Repositories.IRepositories
{
    public interface INewsArticleRepository
    {
        Task<IEnumerable<NewsArticle>> GetAllAsync();
        Task<PagingDto<NewsArticle>> GetListPagingAsync(NewsArticleSearchDto searchDto);
        Task<NewsArticle?> GetByIdAsync<TKey>(TKey id) where TKey : notnull;
        Task<NewsArticle?> GetByIdAsync(string id);
        Task<NewsArticle> AddAsync(NewsArticle newsArticle);
        Task UpdateAsync(NewsArticle newsArticle);
        Task DeleteAsync(NewsArticle newsArticle);
        Task<bool> ExistsByIdAsync(string newsArticleId);
        Task<int> SaveChangesAsync();
    }
}
