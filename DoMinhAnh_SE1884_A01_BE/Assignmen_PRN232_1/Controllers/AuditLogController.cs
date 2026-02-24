using Assignmen_PRN232__.Models;
using Assignmen_PRN232__.Dto.Common;
using Assignmen_PRN232_1.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Assignmen_PRN232_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuditLogController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] AuditLogQueryDto dto)
        {
            var query = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(dto.EntityType))
                query = query.Where(x => x.EntityType == dto.EntityType);

            if (!string.IsNullOrWhiteSpace(dto.Action))
                query = query.Where(x => x.Action == dto.Action);

            if (!string.IsNullOrWhiteSpace(dto.EntityId))
                query = query.Where(x => x.EntityId == dto.EntityId);

            if (dto.PerformedById.HasValue)
                query = query.Where(x => x.PerformedById == dto.PerformedById);

            if (dto.FromDate.HasValue)
                query = query.Where(x => x.Timestamp >= dto.FromDate);

            if (dto.ToDate.HasValue)
                query = query.Where(x => x.Timestamp <= dto.ToDate.Value.AddDays(1));

            var total = await query.CountAsync();

            var pageNumber = dto.PageNumber < 1 ? 1 : dto.PageNumber;
            var pageSize   = dto.PageSize < 1 ? 15 : dto.PageSize;

            var items = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Enrich performer names from accounts where missing
            var ids = items
                .Where(x => x.PerformedById.HasValue && x.PerformedByName == null)
                .Select(x => x.PerformedById!.Value)
                .Distinct()
                .ToList();

            if (ids.Count > 0)
            {
                var names = await _db.SystemAccounts
                    .Where(a => ids.Contains(a.AccountId))
                    .ToDictionaryAsync(a => a.AccountId, a => a.AccountName);

                foreach (var item in items)
                {
                    if (item.PerformedById.HasValue && item.PerformedByName == null)
                        item.PerformedByName = names.GetValueOrDefault(item.PerformedById.Value);
                }

                // Persist the enriched names so next call is faster
                _db.AuditLogs.UpdateRange(items.Where(x => x.PerformedById.HasValue));
                await _db.SaveChangesAsync();
            }

            var result = new PagingDto<AuditLog>
            {
                PageNumber = pageNumber,
                PageSize   = pageSize,
                TotalCount = total,
                Items      = items
            };

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
    }

    public class AuditLogQueryDto
    {
        public string? EntityType { get; set; }
        public string? Action { get; set; }
        public string? EntityId { get; set; }
        public short? PerformedById { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
    }
}
