using MitRestApi.Models;
using System;
using System.Linq;

namespace MitRestApi.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated(); // Opret DB hvis den ikke findes

            // Tjek om der allerede er brugere
            if (context.Users.Any())
            {
                return;   // DB er allerede seeded
            }

            var users = new User[]
            {
                new User { Id = 1, Name = "Christian", BirthDate = new DateOnly(1985, 8, 7), Gender = "Male" },
                new User { Id = 2, Name = "Helle", BirthDate =  new DateOnly(1982, 9, 3), Gender = "Female" }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }
    }
}
