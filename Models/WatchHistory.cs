using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmFusion.Models
{
    [Table("WatchHistories")]
    public class WatchHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string MovieTitle { get; set; }

        [Required]
        public int ProgressPercentage { get; set; }

        public DateTime WatchedAt { get; set; } = DateTime.UtcNow;
    }
}