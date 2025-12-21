using System.ComponentModel.DataAnnotations;

namespace PasswordManager.Models
{
    public class Folder
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public int? ParentFolderId { get; set; }
        public Folder? ParentFolder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Навигация
        public ICollection<Folder> SubFolders { get; set; } = new List<Folder>();
        public ICollection<Password> Passwords { get; set; } = new List<Password>();
    }
}
