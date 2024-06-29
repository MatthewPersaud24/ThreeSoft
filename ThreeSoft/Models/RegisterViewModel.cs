using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;

namespace ThreeSoft.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter a first name.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter a last name.")]
        public string LastName { get; set; }
        
        [Required(ErrorMessage = "Please enter a username.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Please enter a password.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please select an account type.")]
        public string UserType { get; set; }
    }
}