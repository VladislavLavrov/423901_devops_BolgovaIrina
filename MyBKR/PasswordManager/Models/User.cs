using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty; // Инициализация по умолчанию

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty; // Инициализация по умолчанию

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}