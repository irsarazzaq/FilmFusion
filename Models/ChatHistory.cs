using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmFusion.Models
{
    public class ChatHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserQuery { get; set; } // User text input (e.g., "Rough day at work, need a laugh")

        [Required]
        public string AiResponse { get; set; } // Gemini AI recommendation text

        [Required]
        public string ExtractedMood { get; set; } // Detected mood (e.g., "Stressed")

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Relationship Setup
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}