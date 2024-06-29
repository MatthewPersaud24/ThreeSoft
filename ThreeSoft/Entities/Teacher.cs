﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ThreeSoft.Entities
{
    public class Teacher : User
    { 

        public ICollection<Classroom> Classrooms { get; set; }

        public bool isVerified { get; set; } = false;

    }
}