using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ThreeSoft.Entities
{
    public class Classroom
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public User Teacher { get; set; }

        public ICollection<User> Students { get; set; }
    }
}