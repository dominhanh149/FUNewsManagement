using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Dto;
using Assignmen_PRN232_1.DTOs.Common;

namespace Assignmen_PRN232__.Repositories.IRepositories
{
    public interface ISystemAccountRepository
    {
        Task<PagingDto<SystemAccount>> GetListPagingAsync(SystemAccountSearchDto dto);
        Task<IEnumerable<SystemAccount>> GetAllAsync();
        Task<SystemAccount?> GetByIdAsync(short id);
        Task<SystemAccount?> GetByEmailAsync(string email);

        Task<bool> EmailExistsAsync(string email, short? excludeId = null);
        Task<bool> HasCreatedNewsAsync(short accountId);

        Task AddAsync(SystemAccount entity);
        Task UpdateAsync(SystemAccount entity);
        Task DeleteAsync(SystemAccount entity);
        Task<int> SaveChangesAsync();
    }
}
