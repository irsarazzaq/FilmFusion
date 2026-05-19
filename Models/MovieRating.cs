using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmFusion.Models
{
    [Table("MovieRatings")]
    public class MovieRating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string MovieTitle { get; set; }

        [Required]
        [Range(1, 5)]
        public int StarsCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}