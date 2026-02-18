using Assignmen_PRN232__.Models;
using Assignmen_PRN232_1.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace Assignmen_PRN232__.Dto
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string CategoryName { get; set; } = null!;

        [Required(ErrorMessage = "Category description is required")]
        [StringLength(250, ErrorMessage = "Category description cannot exceed 250 characters")]
        public string CategoryDesciption { get; set; } = null!;

        public short? ParentCategoryID { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // DTO for updating existing category
    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Category ID is required")]
        public short CategoryID { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string CategoryName { get; set; } = null!;

        [Required(ErrorMessage = "Category description is required")]
        [StringLength(250, ErrorMessage = "Category description cannot exceed 250 characters")]
        public string CategoryDesciption { get; set; } = null!;

        public short? ParentCategoryID { get; set; }

        public bool IsActive { get; set; }
    }

    // DTO for category response with article count
    public class CategoryResponseDto
    {
        public short CategoryID { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryDesciption { get; set; } = null!;
        public short? ParentCategoryID { get; set; }
        public string? ParentCategoryName { get; set; }
        public bool IsActive { get; set; }
        public int ArticleCount { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    // DTO for simple category info (for dropdown, etc.)
    public class CategorySimpleDto
    {
        public short CategoryID { get; set; }
        public string CategoryName { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    // DTO for search parameters
    public class CategorySearchDto : BaseSearchDto
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public short? ParentCategoryID { get; set; }
    }

    // DTO for toggle active status
    public class ToggleCategoryStatusDto
    {
        [Required]
        public short CategoryID { get; set; }

        [Required]
        public bool IsActive { get; set; }
    }

    // DTO for category with children
    public class CategoryWithChildrenDto
    {
        public short CategoryID { get; set; }
        public string CategoryName { get; set; } = null!;
        public string CategoryDesciption { get; set; } = null!;
        public bool IsActive { get; set; }
        public int ArticleCount { get; set; }
        public List<CategorySimpleDto> ChildCategories { get; set; } = new();
    }
}