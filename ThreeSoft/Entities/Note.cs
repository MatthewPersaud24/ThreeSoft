using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ThreeSoft.Entities
{
    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Content { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }

        public User User { get; set; }

        public bool IsLocked { get; set; }

        public int? ParentNoteId { get; set; }
        public Note ParentNote { get; set; }
        public List<Note> Replies { get; set; } = new List<Note>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}