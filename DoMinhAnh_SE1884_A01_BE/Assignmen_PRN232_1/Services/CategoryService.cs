using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Dto.Common;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.DTOs.Common;
using Assignmen_PRN232_1.Services.IServices;
using Mapster;

namespace Assignmen_PRN232_1.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(
            ICategoryRepository categoryRepository,
            ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        #region Basic CRUD Operations

        public async Task<ApiResponse<CategoryResponseDto>> GetByIdAsync(short id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);

                if (category == null)
                {
                    return ApiResponse<CategoryResponseDto>.ErrorResponse(
                        "Category not found",
                        $"No category found with ID: {id}");
                }

                var articleCount = await _categoryRepository.GetArticleCountAsync(id);

                var response = new CategoryResponseDto
                {
                    CategoryID = category.CategoryId,
                    CategoryName = category.CategoryName,
                    CategoryDesciption = category.CategoryDesciption,
                    ParentCategoryID = category.ParentCategoryId,
                    ParentCategoryName = category.ParentCategory?.CategoryName,
                    IsActive = category.IsActive ?? false,
                    ArticleCount = articleCount
                };

                return ApiResponse<CategoryResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID: {CategoryId}", id);
                return ApiResponse<CategoryResponseDto>.ErrorResponse(
                    "Error retrieving category",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<CategoryResponseDto>>> GetAllAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();

                var response = categories.Select(c => new CategoryResponseDto
                {
                    CategoryID = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryDesciption = c.CategoryDesciption,
                    ParentCategoryID = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory?.CategoryName,
                    IsActive = c.IsActive ?? false,
                    ArticleCount = c.NewsArticles?.Count ?? 0
                }).ToList();

                return ApiResponse<IEnumerable<CategoryResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return ApiResponse<IEnumerable<CategoryResponseDto>>.ErrorResponse(
                    "Error retrieving categories",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<CategoryResponseDto>> CreateAsync(CreateCategoryDto createDto)
        {
            try
            {
                // Validate ParentCategoryID if provided
                if (createDto.ParentCategoryID.HasValue)
                {
                    var parentExists = await _categoryRepository.ExistsAsync(createDto.ParentCategoryID.Value);
                    if (!parentExists)
                    {
                        return ApiResponse<CategoryResponseDto>.ErrorResponse(
                            "Invalid parent category",
                            "The specified parent category does not exist");
                    }
                }

                // Create new category entity
                var category = new Category
                {
                    CategoryName = createDto.CategoryName,
                    CategoryDesciption = createDto.CategoryDesciption,
                    ParentCategoryId = createDto.ParentCategoryID,
                    IsActive = createDto.IsActive
                };

                var createdCategory = await _categoryRepository.CreateAsync(category);

                var response = new CategoryResponseDto
                {
                    CategoryID = createdCategory.CategoryId,
                    CategoryName = createdCategory.CategoryName,
                    CategoryDesciption = createdCategory.CategoryDesciption,
                    ParentCategoryID = createdCategory.ParentCategoryId,
                    IsActive = createdCategory.IsActive ?? false,
                    ArticleCount = 0
                };

                return ApiResponse<CategoryResponseDto>.SuccessResponse(
                    response,
                    "Category created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return ApiResponse<CategoryResponseDto>.ErrorResponse(
                    "Error creating category",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<CategoryResponseDto>> UpdateAsync(UpdateCategoryDto updateDto)
        {
            try
            {
                // Check if category exists
                var existingCategory = await _categoryRepository.GetByIdAsync(updateDto.CategoryID);
                if (existingCategory == null)
                {
                    return ApiResponse<CategoryResponseDto>.ErrorResponse(
                        "Category not found",
                        $"No category found with ID: {updateDto.CategoryID}");
                }

                // Check if ParentCategoryID is being changed
                if (existingCategory.ParentCategoryId != updateDto.ParentCategoryID)
                {
                    // Check if category is used by articles
                    var canChangeParent = await _categoryRepository.CanChangeParentCategoryAsync(updateDto.CategoryID);
                    if (!canChangeParent)
                    {
                        return ApiResponse<CategoryResponseDto>.ErrorResponse(
                            "Cannot change parent category",
                            "This category is already used by articles and its parent category cannot be changed");
                    }

                    // Validate new ParentCategoryID if provided
                    if (updateDto.ParentCategoryID.HasValue)
                    {
                        var parentExists = await _categoryRepository.ExistsAsync(updateDto.ParentCategoryID.Value);
                        if (!parentExists)
                        {
                            return ApiResponse<CategoryResponseDto>.ErrorResponse(
                                "Invalid parent category",
                                "The specified parent category does not exist");
                        }

                        // Prevent circular reference
                        if (updateDto.ParentCategoryID.Value == updateDto.CategoryID)
                        {
                            return ApiResponse<CategoryResponseDto>.ErrorResponse(
                                "Invalid parent category",
                                "A category cannot be its own parent");
                        }
                    }
                }

                // Update category
                existingCategory.CategoryName = updateDto.CategoryName;
                existingCategory.CategoryDesciption = updateDto.CategoryDesciption;
                existingCategory.ParentCategoryId = updateDto.ParentCategoryID;
                existingCategory.IsActive = updateDto.IsActive;

                await _categoryRepository.UpdateAsync(existingCategory);

                var articleCount = await _categoryRepository.GetArticleCountAsync(updateDto.CategoryID);

                var response = new CategoryResponseDto
                {
                    CategoryID = existingCategory.CategoryId,
                    CategoryName = existingCategory.CategoryName,
                    CategoryDesciption = existingCategory.CategoryDesciption,
                    ParentCategoryID = existingCategory.ParentCategoryId,
                    IsActive = existingCategory.IsActive ?? false,
                    ArticleCount = articleCount
                };

                return ApiResponse<CategoryResponseDto>.SuccessResponse(
                    response,
                    "Category updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", updateDto.CategoryID);
                return ApiResponse<CategoryResponseDto>.ErrorResponse(
                    "Error updating category",
                    ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteAsync(short id)
        {
            try
            {
                // Check if category exists
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return ApiResponse.ErrorResponse(
                        "Category not found",
                        $"No category found with ID: {id}");
                }

                // Check if category has articles
                var hasArticles = await _categoryRepository.HasArticlesAsync(id);
                if (hasArticles)
                {
                    return ApiResponse.ErrorResponse(
                        "Cannot delete category",
                        "This category is being used by one or more articles and cannot be deleted");
                }

                // Check if category has child categories
                var childCategories = await _categoryRepository.GetChildCategoriesAsync(id);
                if (childCategories.Any())
                {
                    return ApiResponse.ErrorResponse(
                        "Cannot delete category",
                        "This category has child categories and cannot be deleted");
                }

                await _categoryRepository.DeleteAsync(id);

                return ApiResponse.SuccessResponse("Category deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", id);
                return ApiResponse.ErrorResponse(
                    "Error deleting category",
                    ex.Message);
            }
        }

        #endregion

        #region Search and Filter

        public async Task<ApiResponse<PagingDto<CategoryResponseDto>>> SearchCategoriesAsync(CategorySearchDto searchDto)
        {
            try
            {
                var result = await _categoryRepository.SearchCategoriesAsync(searchDto);
                return ApiResponse<PagingDto<CategoryResponseDto>>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching categories");
                return ApiResponse<PagingDto<CategoryResponseDto>>.ErrorResponse(
                    "Error searching categories",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<CategorySimpleDto>>> GetActiveCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetActiveCategoriesAsync();
                return ApiResponse<IEnumerable<CategorySimpleDto>>.SuccessResponse(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active categories");
                return ApiResponse<IEnumerable<CategorySimpleDto>>.ErrorResponse(
                    "Error retrieving active categories",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<CategorySimpleDto>>> GetParentCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetParentCategoriesAsync();
                return ApiResponse<IEnumerable<CategorySimpleDto>>.SuccessResponse(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parent categories");
                return ApiResponse<IEnumerable<CategorySimpleDto>>.ErrorResponse(
                    "Error retrieving parent categories",
                    ex.Message);
            }
        }

        #endregion

        #region Category Hierarchy

        public async Task<ApiResponse<IEnumerable<CategorySimpleDto>>> GetChildCategoriesAsync(short parentId)
        {
            try
            {
                var parentExists = await _categoryRepository.ExistsAsync(parentId);
                if (!parentExists)
                {
                    return ApiResponse<IEnumerable<CategorySimpleDto>>.ErrorResponse(
                        "Parent category not found",
                        $"No category found with ID: {parentId}");
                }

                var childCategories = await _categoryRepository.GetChildCategoriesAsync(parentId);
                return ApiResponse<IEnumerable<CategorySimpleDto>>.SuccessResponse(childCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting child categories for parent: {ParentId}", parentId);
                return ApiResponse<IEnumerable<CategorySimpleDto>>.ErrorResponse(
                    "Error retrieving child categories",
                    ex.Message);
            }
        }

        public async Task<ApiResponse<CategoryWithChildrenDto>> GetCategoryWithChildrenAsync(short id)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryWithChildrenAsync(id);

                if (category == null)
                {
                    return ApiResponse<CategoryWithChildrenDto>.ErrorResponse(
                        "Category not found",
                        $"No category found with ID: {id}");
                }

                return ApiResponse<CategoryWithChildrenDto>.SuccessResponse(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category with children: {CategoryId}", id);
                return ApiResponse<CategoryWithChildrenDto>.ErrorResponse(
                    "Error retrieving category details",
                    ex.Message);
            }
        }

        #endregion

        #region Toggle Status

        public async Task<ApiResponse> ToggleActiveStatusAsync(ToggleCategoryStatusDto toggleDto)
        {
            try
            {
                var exists = await _categoryRepository.ExistsAsync(toggleDto.CategoryID);
                if (!exists)
                {
                    return ApiResponse.ErrorResponse(
                        "Category not found",
                        $"No category found with ID: {toggleDto.CategoryID}");
                }

                var success = await _categoryRepository.ToggleActiveStatusAsync(
                    toggleDto.CategoryID,
                    toggleDto.IsActive);

                if (success)
                {
                    return ApiResponse.SuccessResponse(
                        $"Category status updated to {(toggleDto.IsActive ? "Active" : "Inactive")}");
                }

                return ApiResponse.ErrorResponse(
                    "Failed to update status",
                    "An error occurred while updating the category status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category status: {CategoryId}", toggleDto.CategoryID);
                return ApiResponse.ErrorResponse(
                    "Error updating category status",
                    ex.Message);
            }
        }

        #endregion

        #region Validation

        public async Task<ApiResponse<int>> GetArticleCountAsync(short categoryId)
        {
            try
            {
                var exists = await _categoryRepository.ExistsAsync(categoryId);
                if (!exists)
                {
                    return ApiResponse<int>.ErrorResponse(
                        "Category not found",
                        $"No category found with ID: {categoryId}");
                }

                var count = await _categoryRepository.GetArticleCountAsync(categoryId);
                return ApiResponse<int>.SuccessResponse(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article count for category: {CategoryId}", categoryId);
                return ApiResponse<int>.ErrorResponse(
                    "Error retrieving article count",
                    ex.Message);
            }
        }

        #endregion
    }
}