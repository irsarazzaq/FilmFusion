using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmFusion.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        public string FullName { get; set; } = "Cluster User";

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } = "USER";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // NEW PROPERTIES FOR PORTAL PREFERENCES & AVATARS
        public string AvatarUrl { get; set; } = "https://api.dicebear.com/7.x/bottts/svg?seed=Felix";

        public string PreferredGenres { get; set; } = "Action,Sci-Fi,Drama";

        public string PreferredGenre { get; set; } = "Sci-Fi";

        // Compatibility fallback for AdminConsole legacy properties
        public string SystemInterests { get; set; } = "Sci-Fi";
    }
}