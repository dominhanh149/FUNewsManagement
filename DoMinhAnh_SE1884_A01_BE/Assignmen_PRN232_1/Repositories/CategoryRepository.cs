using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.Data;
using Assignmen_PRN232_1.DTOs.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Assignmen_PRN232__.Repositories
{
    public class CategoryRepository : BaseRepository<Category, AppDbContext>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext dbContext, IUnitOfWork unitOfWork)
            : base(dbContext, unitOfWork)
        {
        }

        #region Basic CRUD Operations

        public async Task<Category?> GetByIdAsync(short id)
        {
            return await FindByCondition(c => c.CategoryId == id, trackChanges: true)
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await FindAll(trackChanges: false)
                .Include(c => c.ParentCategory)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<Category> CreateAsync(Category category)
        {
            var result = await AddAsync(category);
            await SaveChangesAsync();
            return result;
        }

        public async Task UpdateAsync(Category category)
        {
            await SaveChangesAsync();
        }

        public async Task DeleteAsync(short id)
        {
            var category = await GetByIdAsync(id);
            if (category != null)
            {
                await base.DeleteAsync(category);
                await SaveChangesAsync();
            }
        }

        #endregion

        #region Search and Filter

        public async Task<PagingDto<CategoryResponseDto>> SearchCategoriesAsync(CategorySearchDto searchDto)
        {
            var query = FindAll(trackChanges: false)
                .Include(c => c.ParentCategory)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                var searchTerm = searchDto.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.CategoryName.ToLower().Contains(searchTerm) ||
                    c.CategoryDesciption.ToLower().Contains(searchTerm));
            }

            // Filter by IsActive
            if (searchDto.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == searchDto.IsActive.Value);
            }

            // Filter by ParentCategoryID
            if (searchDto.ParentCategoryID.HasValue)
            {
                query = query.Where(c => c.ParentCategoryId == searchDto.ParentCategoryID.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(searchDto.SortBy))
            {
                var sortDirection = searchDto.SortDirection.ToLower() == "desc" ? "descending" : "ascending";
                //query = query.OrderBy($"{searchDto.SortBy} {sortDirection}");
            }
            else
            {
                query = query.OrderBy(c => c.CategoryName);
            }

            // Apply pagination
            var items = await query
                .Skip(searchDto.Skip)
                .Take(searchDto.PageSize)
                .Select(c => new CategoryResponseDto
                {
                    CategoryID = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryDesciption = c.CategoryDesciption,
                    ParentCategoryID = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.CategoryName : null,
                    IsActive = c.IsActive ?? false,
                    ArticleCount = c.NewsArticles.Count
                })
                .ToListAsync();

            return new PagingDto<CategoryResponseDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize
            };
        }

        public async Task<IEnumerable<CategorySimpleDto>> GetActiveCategoriesAsync()
        {
            return await FindByCondition(c => c.IsActive == true, trackChanges: false)
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategorySimpleDto
                {
                    CategoryID = c.CategoryId,
                    CategoryName = c.CategoryName,
                    IsActive = c.IsActive ?? false
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CategorySimpleDto>> GetParentCategoriesAsync()
        {
            return await FindByCondition(c => c.ParentCategoryId == null, trackChanges: false)
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategorySimpleDto
                {
                    CategoryID = c.CategoryId,
                    CategoryName = c.CategoryName,
                    IsActive = c.IsActive ?? false
                })
                .ToListAsync();
        }

        #endregion

        #region Validation and Checks

        public async Task<bool> ExistsAsync(short id)
        {
            return await ExistsAsync(c => c.CategoryId == id);
        }

        public async Task<bool> HasArticlesAsync(short categoryId)
        {
            return await _dbContext.NewsArticles
                .AnyAsync(n => n.CategoryId == categoryId);
        }

        public async Task<bool> IsCategoryUsedByArticlesAsync(short categoryId)
        {
            return await HasArticlesAsync(categoryId);
        }

        public async Task<int> GetArticleCountAsync(short categoryId)
        {
            return await _dbContext.NewsArticles
                .CountAsync(n => n.CategoryId == categoryId);
        }

        #endregion

        #region Category Hierarchy

        public async Task<IEnumerable<CategorySimpleDto>> GetChildCategoriesAsync(short parentId)
        {
            return await FindByCondition(c => c.ParentCategoryId == parentId, trackChanges: false)
                .OrderBy(c => c.CategoryName)
                .Select(c => new CategorySimpleDto
                {
                    CategoryID = c.CategoryId,
                    CategoryName = c.CategoryName,
                    IsActive = c.IsActive ?? false
                })
                .ToListAsync();
        }

        public async Task<CategoryWithChildrenDto?> GetCategoryWithChildrenAsync(short id)
        {
            var category = await FindByCondition(c => c.CategoryId == id, trackChanges: false)
                .Include(c => c.InverseParentCategory)
                .FirstOrDefaultAsync();

            if (category == null)
                return null;

            var articleCount = await GetArticleCountAsync(id);

            return new CategoryWithChildrenDto
            {
                CategoryID = category.CategoryId,
                CategoryName = category.CategoryName,
                CategoryDesciption = category.CategoryDesciption,
                IsActive = category.IsActive ?? false,
                ArticleCount = articleCount,
                ChildCategories = category.InverseParentCategory
                    .Select(c => new CategorySimpleDto
                    {
                        CategoryID = c.CategoryId,
                        CategoryName = c.CategoryName,
                        IsActive = c.IsActive ?? false
                    })
                    .ToList()
            };
        }

        #endregion

        #region Toggle Status

        public async Task<bool> ToggleActiveStatusAsync(short categoryId, bool isActive)
        {
            var category = await FindByCondition(c => c.CategoryId == categoryId, trackChanges: true)
                .FirstOrDefaultAsync();

            if (category == null)
                return false;

            category.IsActive = isActive;
            await SaveChangesAsync();
            return true;
        }

        #endregion

        #region Check Parent Category Change

        public async Task<bool> CanChangeParentCategoryAsync(short categoryId)
        {
            // A category's parent cannot be changed if it's already used by articles
            var hasArticles = await HasArticlesAsync(categoryId);
            return !hasArticles;
        }

        #endregion
    }
}