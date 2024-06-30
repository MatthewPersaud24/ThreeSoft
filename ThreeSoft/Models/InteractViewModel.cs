using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ThreeSoft.Entities;

namespace ThreeSoft.Models
{
    public class InteractViewModel
    {
        public User Student { get; set; }
        public List<Note> Notes { get; set; }
        public List<Checklist> Checklists { get; set; }
        public ParentPinViewModel Parent { get; set; }
    }
}
