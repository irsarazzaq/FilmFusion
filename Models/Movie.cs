using System;
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
        public string PosterUrl { get; set; } = string.Empty;

        public string VideoUrl { get; set; } = string.Empty;

        [Required]
        public int TargetAgeLimit { get; set; } = 13;

        [Required]
        public string Description { get; set; } = string.Empty;

        public bool IsTrending { get; set; } = false;

        public double TmdbRating { get; set; } = 0.0;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
