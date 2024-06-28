using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ThreeSoft.Entities
{
    public class ChecklistTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Task { get; set; }

        public bool IsCompleted { get; set; }

        [ForeignKey("Checklist")]
        public int ChecklistId { get; set; }

        public Checklist Checklist { get; set; }
    }
}