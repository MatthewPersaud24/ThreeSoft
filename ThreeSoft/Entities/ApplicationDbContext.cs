using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThreeSoft.Entities
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Checklist> Checklists { get; set; }
        public DbSet<ChecklistTask> ChecklistTasks { get; set; }

        // Other DbSet properties...

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });

            modelBuilder.Entity<Classroom>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.TeacherClassrooms)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Classroom>()
                .HasMany(c => c.Students)
                .WithMany(s => s.StudentClassrooms)
                .UsingEntity<Dictionary<string, object>>(
                    "ClassroomStudent",
                    r => r.HasOne<User>().WithMany().HasForeignKey("StudentId"),
                    l => l.HasOne<Classroom>().WithMany().HasForeignKey("ClassroomId"),
                    je =>
                    {
                        je.HasKey("ClassroomId", "StudentId");
                        je.ToTable("ClassroomStudent");
                    });

            // Seed roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "Teacher", NormalizedName = "TEACHER" },
                new IdentityRole { Name = "Student", NormalizedName = "STUDENT" }
            );
        }

        public static async Task CreateStaticUsers(IServiceProvider serviceProvider)
        {
            UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure roles exist
            string[] roleNames = { "Admin", "Teacher", "Student" };
            foreach (var roleName in roleNames)
            {
                if (await roleManager.FindByNameAsync(roleName) == null)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Static Admin user
            User staticAdmin = new User { UserName = "Admin", FirstName = "Ronald", LastName = "McDonald", ParentPin = "defaultPin", isVerified = true };
            if (await userManager.FindByNameAsync(staticAdmin.UserName) == null)
            {
                var result = await userManager.CreateAsync(staticAdmin, "Admin@1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staticAdmin, "Admin");
                }
            }

            // Static Teacher user
            User staticTeacher = new User { UserName = "TeacherUser", FirstName = "Teacher", LastName = "Example" , isVerified = true };
            if (await userManager.FindByNameAsync(staticTeacher.UserName) == null)
            {
                var result = await userManager.CreateAsync(staticTeacher, "Teacher@1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staticTeacher, "Teacher");
                }
            }

            // Static Student user
            User staticStudent = new User { UserName = "StudentUser", FirstName = "Student", LastName = "Example", isVerified = true };
            if (await userManager.FindByNameAsync(staticStudent.UserName) == null)
            {
                var result = await userManager.CreateAsync(staticStudent, "Student@1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staticStudent, "Student");
                }
            }
        }
    }
}