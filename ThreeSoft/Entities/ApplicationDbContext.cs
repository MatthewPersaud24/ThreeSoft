using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ThreeSoft.Entities
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Note> Notes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            // Define any additional configuration or relationships here
        }

        public static async Task CreateStaticUsers(IServiceProvider serviceProvider)
        {
            UserManager<User> userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Static teacher
            Teacher staticTeacher = new Teacher { UserName = "Admin", firstName = "Ronald", lastName = "McDonald"};
            // if Teacher role doesn't exist, create it
            if (await roleManager.FindByNameAsync("Teacher") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Teacher"));
            }
            // if username doesn't exist, create it and add it to role
            if (await userManager.FindByNameAsync(staticTeacher.UserName) == null)
            {
                var result = await userManager.CreateAsync(staticTeacher, "Admin@1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staticTeacher, "Teacher");
                }
            }

            // Static student
            Student staticStudent = new Student { UserName = "NotBatman", firstName = "Bruce", lastName = "Wayne" };
            // if Student role doesn't exist, create it
            if (await roleManager.FindByNameAsync("Student") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Student"));
            }
            // if username doesn't exist, create it and add it to role
            if (await userManager.FindByNameAsync(staticStudent.UserName) == null)
            {
                var result = await userManager.CreateAsync(staticStudent, "Manbat1!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staticStudent, "Student");
                }
            }

            // Static parent
            Parent staticParent = new Parent { UserName = "DeadParent", firstName = "Thomas", lastName = "Wayne" };
            // if Parent role doesn't exist, create it
            if (await roleManager.FindByNameAsync("Parent") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("Parent"));
            }
            // if username doesn't exist, create it and add it to role
            if (await userManager.FindByNameAsync(staticParent.UserName) == null)
            {
                var result = await userManager.CreateAsync(staticParent, "Deaddad!0");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(staticParent, "Parent");
                }
            }
        }
    }
}
