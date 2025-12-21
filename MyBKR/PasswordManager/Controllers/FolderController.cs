using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Data;
using PasswordManager.Models;
using System.Security.Claims;

namespace PasswordManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔐 Только авторизованные пользователи
    public class FolderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FolderController(AppDbContext context)
        {
            _context = context;
        }

        // Возвращает все папки пользователя с подпапками и паролями
        [HttpGet]
        public async Task<IActionResult> GetFolders()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var allFolders = await _context.Folders
                .Where(f => f.UserId == userId)
                .Include(f => f.SubFolders)
                    .ThenInclude(sf => sf.Passwords)
                .Include(f => f.Passwords)
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.ParentFolderId,
                    f.CreatedAt,
                    SubFolders = f.SubFolders.Select(sf => new
                    {
                        sf.Id,
                        sf.Name,
                        sf.ParentFolderId,
                        sf.CreatedAt,
                        Passwords = sf.Passwords.Select(p => new
                        {
                            p.Id,
                            p.Title,
                            p.Username,
                            p.Url,
                            p.Notes,
                            p.CreatedAt
                        })
                    }),
                    Passwords = f.Passwords.Select(p => new
                    {
                        p.Id,
                        p.Title,
                        p.Username,
                        p.Url,
                        p.Notes,
                        p.CreatedAt
                    })
                })
                .ToListAsync();

            return Ok(allFolders);
        }

        // Создаёт новую папку
        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] FolderDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Имя папки обязательно" });

            var userId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);

            // Проверяем, что родительская папка существует и принадлежит пользователю
            if (dto.ParentFolderId.HasValue)
            {
                var parent = await _context.Folders
                    .FirstOrDefaultAsync(f => f.Id == dto.ParentFolderId.Value && f.UserId == userId);

                if (parent == null)
                    return BadRequest(new { message = "Родительская папка не найдена или недоступна" });
            }

            var folder = new Folder
            {
                Name = dto.Name,
                UserId = userId,
                ParentFolderId = dto.ParentFolderId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFolders), new { id = folder.Id }, new { id = folder.Id, name = folder.Name, parentId = folder.ParentFolderId });
        }

        // Обновляет имя папки
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFolder(int id, [FromBody] FolderDto dto)
        {
            var userId = int.Parse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value);

            var folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (folder == null)
                return NotFound(new { message = "Папка не найдена" });

            if (!string.IsNullOrWhiteSpace(dto.Name))
                folder.Name = dto.Name;

            _context.Folders.Update(folder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Удаляет папку (и всё содержимое — благодаря каскадному удалению)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var folder = await _context.Folders
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

            if (folder == null)
                return NotFound();

            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO для передачи данных при создании/обновлении папки
    public class FolderDto
    {
        public string Name { get; set; }
        public int? ParentFolderId { get; set; }
    }
}