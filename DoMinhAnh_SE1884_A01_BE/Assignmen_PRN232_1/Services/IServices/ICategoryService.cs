using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Dto.Common;
using Assignmen_PRN232_1.DTOs.Common;

namespace Assignmen_PRN232_1.Services.IServices
{
    public interface ICategoryService
    {
        Task<ApiResponse<CategoryResponseDto>> GetByIdAsync(short id);
        Task<ApiResponse<IEnumerable<CategoryResponseDto>>> GetAllAsync();
        Task<ApiResponse<CategoryResponseDto>> CreateAsync(CreateCategoryDto createDto);
        Task<ApiResponse<CategoryResponseDto>> UpdateAsync(UpdateCategoryDto updateDto);
        Task<ApiResponse> DeleteAsync(short id);

        // Search and filter
        Task<ApiResponse<PagingDto<CategoryResponseDto>>> SearchCategoriesAsync(CategorySearchDto searchDto);
        Task<ApiResponse<IEnumerable<CategorySimpleDto>>> GetActiveCategoriesAsync();
        Task<ApiResponse<IEnumerable<CategorySimpleDto>>> GetParentCategoriesAsync();

        // Category hierarchy
        Task<ApiResponse<IEnumerable<CategorySimpleDto>>> GetChildCategoriesAsync(short parentId);
        Task<ApiResponse<CategoryWithChildrenDto>> GetCategoryWithChildrenAsync(short id);

        // Toggle status
        Task<ApiResponse> ToggleActiveStatusAsync(ToggleCategoryStatusDto toggleDto);

        // Validation
        Task<ApiResponse<int>> GetArticleCountAsync(short categoryId);
    }
}
