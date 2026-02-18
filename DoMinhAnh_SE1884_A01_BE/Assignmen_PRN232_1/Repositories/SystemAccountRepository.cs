using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Repositories.IRepositories;
using Assignmen_PRN232_1.Data;
using Assignmen_PRN232_1.DTOs.Common;
using Microsoft.EntityFrameworkCore;

namespace Assignmen_PRN232__.Repositories
{
    public class SystemAccountRepository : ISystemAccountRepository
    {
        private readonly AppDbContext _context;

        public SystemAccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagingDto<SystemAccount>> GetListPagingAsync(SystemAccountSearchDto dto)
        {
            var query = _context.SystemAccounts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.Keyword))
            {
                query = query.Where(x =>
                    x.AccountName!.Contains(dto.Keyword) ||
                    x.AccountEmail!.Contains(dto.Keyword));
            }

            if (dto.Role.HasValue)
                query = query.Where(x => x.AccountRole == dto.Role);

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.AccountId)
                .Skip((dto.PageNumber - 1) * dto.PageSize)
                .Take(dto.PageSize)
                .ToListAsync();

            return new PagingDto<SystemAccount>
            {
                PageNumber = dto.PageNumber,
                PageSize = dto.PageSize,
                TotalCount = total,
                Items = items
            };
        }

        public async Task<IEnumerable<SystemAccount>> GetAllAsync()
            => await _context.SystemAccounts.ToListAsync();

        public async Task<SystemAccount?> GetByIdAsync(short id)
            => await _context.SystemAccounts.FindAsync(id);

        public async Task<SystemAccount?> GetByEmailAsync(string email)
            => await _context.SystemAccounts
                .FirstOrDefaultAsync(x => x.AccountEmail == email);

        public async Task<bool> EmailExistsAsync(string email, short? excludeId = null)
        {
            var query = _context.SystemAccounts
                .Where(x => x.AccountEmail == email);

            if (excludeId.HasValue)
                query = query.Where(x => x.AccountId != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> HasCreatedNewsAsync(short accountId)
        {
            return await _context.NewsArticles
                .AnyAsync(x => x.CreatedById == accountId);
        }

        public async Task AddAsync(SystemAccount entity)
            => await _context.SystemAccounts.AddAsync(entity);

        public Task UpdateAsync(SystemAccount entity)
        {
            _context.SystemAccounts.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(SystemAccount entity)
        {
            _context.SystemAccounts.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<int> SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
