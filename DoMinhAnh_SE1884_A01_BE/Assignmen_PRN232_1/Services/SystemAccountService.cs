using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Dto.Common;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.DTOs.Common;
using Assignmen_PRN232_1.Services.IServices;
using Microsoft.Extensions.Configuration;

namespace Assignmen_PRN232_1.Services
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly ISystemAccountRepository _repo;
        private readonly IConfiguration _config;

        public SystemAccountService(
            ISystemAccountRepository repo,
            IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<PagingDto<SystemAccountDto>> GetListPagingAsync(SystemAccountSearchDto dto)
        {
            var result = await _repo.GetListPagingAsync(dto);
            return new PagingDto<SystemAccountDto>
            {
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Items = result.Items.Select(MapToDto).ToList()
            };
        }

        public async Task<IEnumerable<SystemAccountDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(MapToDto);

        public async Task<SystemAccountDto?> GetByIdAsync(short id)
        {
            var acc = await _repo.GetByIdAsync(id);
            return acc == null ? null : MapToDto(acc);
        }

        private async Task<short> GenerateAccountIdAsync()
        {
            var allIds = (await _repo.GetAllAsync())
                .Select(x => x.AccountId);

            short max = 0;
            foreach (var id in allIds)
            {
                if (id > max)
                    max = id;
            }

            return (short)(max + 1);
        }

        public async Task<ApiResponse<SystemAccountDto>> CreateOrEditAsync(SystemAccountSaveDto dto)
        {
            // check duplicate email
            if (await _repo.EmailExistsAsync(dto.AccountEmail,
                dto.AccountId == 0 ? null : dto.AccountId))
            {
                return ApiResponse<SystemAccountDto>
                    .ErrorResponse("Duplicate email is not allowed.");
            }

            // ===== CREATE =====
            if (dto.AccountId == 0)
            {
                var entity = new SystemAccount
                {
                    AccountId = await GenerateAccountIdAsync(), // 👈 AUTO GEN ID
                    AccountName = dto.AccountName,
                    AccountEmail = dto.AccountEmail,
                    AccountPassword = dto.AccountPassword!,
                    AccountRole = dto.AccountRole
                };

                await _repo.AddAsync(entity);
                await _repo.SaveChangesAsync();

                return ApiResponse<SystemAccountDto>
                    .SuccessResponse(MapToDto(entity), "Created successfully.");
            }

            // ===== UPDATE =====
            var existing = await _repo.GetByIdAsync(dto.AccountId);
            if (existing == null)
                return ApiResponse<SystemAccountDto>.ErrorResponse("Account not found.");

            existing.AccountName = dto.AccountName;
            existing.AccountEmail = dto.AccountEmail;
            existing.AccountRole = dto.AccountRole;

            if (!string.IsNullOrWhiteSpace(dto.AccountPassword))
                existing.AccountPassword = dto.AccountPassword;

            await _repo.UpdateAsync(existing);
            await _repo.SaveChangesAsync();

            return ApiResponse<SystemAccountDto>
                .SuccessResponse(MapToDto(existing), "Updated successfully.");
        }


        public async Task<ApiResponse<bool>> DeleteAsync(short id)
        {
            var acc = await _repo.GetByIdAsync(id);
            if (acc == null)
                return ApiResponse<bool>.ErrorResponse("Account not found.");

            if (await _repo.HasCreatedNewsAsync(id))
                return ApiResponse<bool>
                    .ErrorResponse("Cannot delete account that created news articles.");

            await _repo.DeleteAsync(acc);
            await _repo.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true, "Deleted successfully.");
        }

        public async Task<ApiResponse<SystemAccountDto>> LoginAsync(SystemAccountLoginDto dto)
        {
            // Admin from appsettings
            var adminEmail = _config["AdminAccount:Email"];
            var adminPass = _config["AdminAccount:Password"];

            if (dto.AccountEmail == adminEmail &&
                dto.AccountPassword == adminPass)
            {
                return ApiResponse<SystemAccountDto>.SuccessResponse(
                    new SystemAccountDto
                    {
                        AccountId = 0,
                        AccountEmail = adminEmail,
                        AccountRole = 0
                    });
            }

            var acc = await _repo.GetByEmailAsync(dto.AccountEmail);
            if (acc == null || acc.AccountPassword != dto.AccountPassword)
                return ApiResponse<SystemAccountDto>
                    .ErrorResponse("Invalid email or password.");

            return ApiResponse<SystemAccountDto>.SuccessResponse(MapToDto(acc));
        }

        private static SystemAccountDto MapToDto(SystemAccount x)
            => new SystemAccountDto
            {
                AccountId = x.AccountId,
                AccountName = x.AccountName,
                AccountEmail = x.AccountEmail,
                AccountRole = x.AccountRole
            };
    }
}
