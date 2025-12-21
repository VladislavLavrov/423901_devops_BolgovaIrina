using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Models
{
    public class Password
    {
        public int Id { get; set; }

        public int? FolderId { get; set; }
        public Folder? Folder { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        public string EncryptedPassword { get; set; }

        [StringLength(255)]
        public string? Url { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
