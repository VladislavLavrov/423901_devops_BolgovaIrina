using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Data;
using PasswordManager.Models;
using PasswordManager.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace PasswordManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PasswordController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AesEncryptionService _aes;

        public PasswordController(AppDbContext context, AesEncryptionService aes)
        {
            _context = context;
            _aes = aes;
        }

        // Возвращает список паролей пользователя (всех или из указанной папки)
        [HttpGet]
        public async Task<IActionResult> GetPasswords(int? folderId)
        {
            var userId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);

            IQueryable<Password> query = _context.Passwords
                .Where(p => p.UserId == userId);

            if (folderId.HasValue)
            {
                query = query.Where(p => p.FolderId == folderId);
            }

            var passwords = await query
                .Include(p => p.Folder)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Username,
                    p.Url,
                    p.Notes,
                    p.CreatedAt,
                    p.FolderId,
                    FolderName = p.Folder.Name,
                      p.EncryptedPassword
                })
                .ToListAsync();

            return Ok(passwords);
        }

        // Создаёт новый зашифрованный пароль
        [HttpPost]
        public async Task<IActionResult> CreatePassword([FromBody] CreatePasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Заполните все поля" });

            var userId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);

            // Проверка что папка существует и принадлежит пользователю
            if (dto.FolderId.HasValue)
            {
                var folder = await _context.Folders
                    .FirstOrDefaultAsync(f => f.Id == dto.FolderId.Value && f.UserId == userId);
                if (folder == null)
                    return BadRequest(new { message = "Папка не найдена" });
            }
            // Шифруем пароль с помощью мастер-пароля
            var encryptedPassword = _aes.Encrypt(dto.Password, dto.MasterPassword);

            var password = new Password
            {
                Title = dto.Title,
                Username = dto.Username,
                EncryptedPassword = encryptedPassword,
                Url = dto.Url,
                Notes = dto.Notes,
                FolderId = dto.FolderId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Passwords.Add(password);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPasswords), new { id = password.Id }, new { id = password.Id });
        }

        // Обновляет пароль 
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);

            var password = await _context.Passwords
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (password == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.Title)) password.Title = dto.Title;
            if (!string.IsNullOrWhiteSpace(dto.Username)) password.Username = dto.Username;
            if (!string.IsNullOrWhiteSpace(dto.Url)) password.Url = dto.Url;
            if (dto.Notes != null) password.Notes = dto.Notes;

            // Проверка папки
            if (dto.FolderId.HasValue)
            {
                var folder = await _context.Folders.FirstOrDefaultAsync(f => f.Id == dto.FolderId.Value && f.UserId == userId);
                if (folder != null) password.FolderId = dto.FolderId;
            }
            // Если передан новый пароль — шифруем и сохраняем
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                password.EncryptedPassword = _aes.Encrypt(dto.Password, dto.MasterPassword);
            }

            _context.Passwords.Update(password);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Удаляет пароль
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePassword(int id)
        {
            var userId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);

            var password = await _context.Passwords
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (password == null)
                return NotFound();

            _context.Passwords.Remove(password);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
    // DTO для создания пароля
    public class CreatePasswordDto
    {
        public string Title { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string MasterPassword { get; set; }
        public string? Url { get; set; }
        public string? Notes { get; set; }
        public int? FolderId { get; set; }
    }
    // DTO для обновления — наследуется от CreatePasswordDto
    public class UpdatePasswordDto : CreatePasswordDto { }
}