using System.ComponentModel.DataAnnotations;

namespace FilmFusion.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string? Role { get; set; } // "Admin" ya "User" (Nullable taake migration issue na kare)

        public int Age { get; set; }
    }
}