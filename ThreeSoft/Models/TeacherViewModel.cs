using System.Collections.Generic;
using ThreeSoft.Entities;

namespace ThreeSoft.Models
{
    public class TeacherViewModel
    {
        public List<Classroom> Classrooms { get; set; }
        public List<User> Students { get; set; }
        public string SearchTerm { get; set; }
        public List<User> SearchResults { get; set; }
    }
}