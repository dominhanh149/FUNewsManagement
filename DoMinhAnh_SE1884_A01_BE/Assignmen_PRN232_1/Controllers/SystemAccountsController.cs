using Assignmen_PRN232__.Dto;
using Assignmen_PRN232__.Enums;
using Assignmen_PRN232__.Models;
using Assignmen_PRN232_1.Services.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignmen_PRN232_1.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class SystemAccountsController : ControllerBase
    {
        private readonly ISystemAccountService _service;
        private readonly AppDbContext _db;
        private readonly JwtTokenService _jwt;

        public SystemAccountsController(ISystemAccountService service, AppDbContext db, JwtTokenService jwt)
        {
            _service = service;
            _db = db;
            _jwt = jwt;
        }

        [HttpGet]
        public async Task<IActionResult> GetListPaging([FromQuery] SystemAccountSearchDto dto)
        {
            var result = await _service.GetListPagingAsync(dto);
            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(short id)
        {
            var acc = await _service.GetByIdAsync(id);
            if (acc == null) return NotFound();
            return Ok(acc);
        }

        [HttpPost("create-or-edit")]
        public async Task<IActionResult> CreateOrEdit([FromBody] SystemAccountSaveDto dto)
        {
            var response = await _service.CreateOrEditAsync(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(short id)
        {
            var response = await _service.DeleteAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] SystemAccountLoginDto dto)
        {
            var response = await _service.LoginAsync(dto);
            if (!response.Success)
                return StatusCode(response.StatusCode, response);

            var acc = response.Data;

            var role =
                acc.AccountRole == 0 ? "Admin" :
                acc.AccountRole == 1 ? "Staff" : "Lecturer";

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, acc.AccountId.ToString()),
        new Claim(ClaimTypes.Name, acc.AccountEmail!),
        new Claim(ClaimTypes.Role, role)
    };

            var accessToken = _jwt.GenerateAccessToken(claims);
            var refreshToken = _jwt.GenerateRefreshToken();
            var refreshDays = _jwt.RefreshTokenDays;

            // Admin account (id=0) lives only in appsettings — no DB row → skip FK insert
            if (acc.AccountId != 0)
            {
                var tokenEntity = new RefreshToken
                {
                    AccountId = acc.AccountId,
                    Token = refreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(refreshDays)
                };
                _db.RefreshTokens.Add(tokenEntity);
                await _db.SaveChangesAsync();
            }

            return Ok(new
            {
                success = true,
                access_token = accessToken,
                refresh_token = refreshToken,
                role
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDto dto)
        {
            var existing = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);
            if (existing != null && existing.RevokedAt == null)
            {
                existing.RevokedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            return Ok(new { success = true });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
        {
            var existing = await _db.RefreshTokens
                .Include(x => x.Account)
                .FirstOrDefaultAsync(x => x.Token == dto.RefreshToken);

            if (existing == null) return Unauthorized(new { success = false, message = "Invalid refresh token" });
            if (existing.RevokedAt != null) return Unauthorized(new { success = false, message = "Token revoked" });
            if (existing.ExpiresAt <= DateTime.UtcNow) return Unauthorized(new { success = false, message = "Token expired" });

            var acc = existing.Account;
            var role =
                acc.AccountRole == 0 ? "Admin" :
                acc.AccountRole == 1 ? "Staff" : "Lecturer";

            // rotate refresh token
            var newRefreshToken = _jwt.GenerateRefreshToken();
            existing.RevokedAt = DateTime.UtcNow;
            existing.ReplacedByToken = newRefreshToken;

            _db.RefreshTokens.Add(new RefreshToken
            {
                AccountId = acc.AccountId,
                Token = newRefreshToken,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
            });

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, acc.AccountId.ToString()),
            new Claim(ClaimTypes.Name, acc.AccountEmail ?? ""),
            new Claim(ClaimTypes.Role, role)
        };

            var newAccessToken = _jwt.GenerateAccessToken(claims);

            await _db.SaveChangesAsync();

            return Ok(new { success = true, access_token = newAccessToken, refresh_token = newRefreshToken });
        }
    }
}
