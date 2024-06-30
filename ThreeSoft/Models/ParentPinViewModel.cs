using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ThreeSoft.Entities;

namespace ThreeSoft.Models
{
    public class ParentPinViewModel
    {
        public string? ParentPin { get; set; }
        public bool ParentUnlocked { get; set; } = false;
    }
}
