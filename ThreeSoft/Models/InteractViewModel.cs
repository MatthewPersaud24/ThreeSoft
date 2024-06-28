using System.Collections.Generic;
using ThreeSoft.Entities;

namespace ThreeSoft.Models
{
    public class InteractViewModel
    {
        public User Student { get; set; }
        public List<Note> Notes { get; set; }
        public List<Checklist> Checklists { get; set; }
    }
}
