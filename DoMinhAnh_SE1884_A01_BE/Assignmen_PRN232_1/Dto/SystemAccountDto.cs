using Assignmen_PRN232_1.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace Assignmen_PRN232__.Dto
{
    public class SystemAccountDto
    {
        public short AccountId { get; set; }

        public string? AccountName { get; set; }

        public string? AccountEmail { get; set; }

        public int? AccountRole { get; set; }

        public string? AccountRoleName { get; set; }
    }

    public class CreateSystemAccountDto
    {
        [Required, StringLength(100)]
        public string AccountName { get; set; } = null!;

        [Required, EmailAddress, StringLength(150)]
        public string AccountEmail { get; set; } = null!;

        [Required, StringLength(100, MinimumLength = 6)]
        public string AccountPassword { get; set; } = null!;

        [Required]
        [Range(1, 2, ErrorMessage = "AccountRole must be 1 (Staff) or 2 (Lecturer).")]
        public short AccountRole { get; set; }
    }

    public class UpdateSystemAccountDto
    {
        [Required]
        public short AccountId { get; set; }

        [Required, StringLength(100)]
        public string AccountName { get; set; } = null!;

        [Required, EmailAddress, StringLength(150)]
        public string AccountEmail { get; set; } = null!;

        [Range(1, 2)]
        public short? AccountRole { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required]
        public short AccountId { get; set; }

        [Required]
        public string CurrentPassword { get; set; } = null!;

        [Required, StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = null!;
    }

    public class SystemAccountSearchDto : BaseSearchDto
    {
        public string? Keyword { get; set; }
        public short? Role { get; set; }
    }

    // Login
    public class SystemAccountLoginDto
    {
        [Required, EmailAddress]
        public string AccountEmail { get; set; } = null!;

        [Required]
        public string AccountPassword { get; set; } = null!;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = null!;
        public string Role { get; set; } = null!;
        public short? AccountId { get; set; }
        public string? AccountEmail { get; set; }
        public string? AccountName { get; set; }
    }
    public class SystemAccountSaveDto
    {
        public short AccountId { get; set; }

        [Required]
        public string AccountName { get; set; } = null!;

        [Required, EmailAddress]
        public string AccountEmail { get; set; } = null!;

        public string? AccountPassword { get; set; }

        public short AccountRole { get; set; }
    }
}
