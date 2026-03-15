using InternProject.Data;
using InternProject.Models.UserModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InternProject.Tests.Helpers
{
    public static class TestDataSeeder
    {
        public static async Task SeedUserAsync(AppDbContext db)
        {
            // Add test data seeding logic here
            // For example, you can add users, products, orders, etc.
            // Example: Adding a test user
            if (!await db.Users.AnyAsync(u=>u.Email == "test@example.com"))
            {
                db.Users.Add(new User
                {
                    UserId = Guid.NewGuid(),
                    UserName = "testuser",
                    Email = "test@example.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                    Status = AccountStatus.Active,
                    IsEmailVerified = true
                });
                await db.SaveChangesAsync();
            }
        }
    }
}
