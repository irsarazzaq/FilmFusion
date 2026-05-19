using System.ComponentModel.DataAnnotations;

namespace FilmFusion.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Genre { get; set; } = string.Empty;

        [Required]
        public string Duration { get; set; } = string.Empty;

        [Required]
        public string? VideoUrl { get; set; } // Movie play karne ke liye streaming link node

        [Required]
        public string PosterUrl { get; set; } = string.Empty;

        // Smart Age Gate Fields
        [Required]
        public int TargetAgeLimit { get; set; } = 13; // Default 13+ content classification

        [Required]
        public string Description { get; set; } = string.Empty;
    }
}