using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ThreeSoft.Entities
{
    public class Admin : User
    {
        [Required(ErrorMessage = "Please enter a first name")]
        public string? firstName { get; set; }

        [Required(ErrorMessage = "Please enter a last name")]
        public string? lastName { get; set; }
    }
}