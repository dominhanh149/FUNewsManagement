using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232_1.DTOs.Common;

namespace Assignmen_PRN232__.Repositories.IRepositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(short id);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(short id);

        // Search and filter
        Task<PagingDto<CategoryResponseDto>> SearchCategoriesAsync(CategorySearchDto searchDto);
        Task<IEnumerable<CategorySimpleDto>> GetActiveCategoriesAsync();
        Task<IEnumerable<CategorySimpleDto>> GetParentCategoriesAsync();

        // Validation and checks
        Task<bool> ExistsAsync(short id);
        Task<bool> HasArticlesAsync(short categoryId);
        Task<bool> IsCategoryUsedByArticlesAsync(short categoryId);
        Task<int> GetArticleCountAsync(short categoryId);

        // Category hierarchy
        Task<IEnumerable<CategorySimpleDto>> GetChildCategoriesAsync(short parentId);
        Task<CategoryWithChildrenDto?> GetCategoryWithChildrenAsync(short id);

        // Toggle status
        Task<bool> ToggleActiveStatusAsync(short categoryId, bool isActive);

        // Check if ParentCategoryID can be changed
        Task<bool> CanChangeParentCategoryAsync(short categoryId);
    }
}
