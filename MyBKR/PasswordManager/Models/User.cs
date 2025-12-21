using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигация
        public ICollection<Folder> Folders { get; set; }
        public ICollection<Password> Passwords { get; set; }
    }
}
