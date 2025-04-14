using System;
using System.Collections.Generic;
using System.Linq;
using API.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design.Serialization;
using System.Text.Json;
using API.Entities;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUser(UserManager<AppUser> userManager,RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var userData  = await File.ReadAllTextAsync("Data/UserSeedData.json");

            var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};

            var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

            var roles = new List<AppRole>
            {
                new() {Name = "Member"},
                new() {Name = "Admin"},
                new() {Name = "Moderator"}
            };

            foreach (var role in roles)
             {
                await roleManager.CreateAsync(role);
             }

            if (users == null) return;

            foreach (var user in users)
            {
                // using var hmac = new HMACSHA512();

                // user.UserName = user.UserName.ToLower();
                // user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes ("Pa$$w0rd"));
                // user.PasswordSalt = hmac.Key;

                // context.Users.Add(user);
                user.UserName = user.UserName!.ToLower();
                await userManager.CreateAsync(user ,"Pa$$w0rd" );
                await userManager.AddToRoleAsync(user, "Member");

            }

            var admin = new AppUser
            {
                UserName = "admin",
                knownAs = "Admin",
                Gender = "",
                City = "",
                Country = ""

            };
            await userManager.CreateAsync(admin ,"Pa$$w0rd" );
            await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);

            // await context.SaveChangesAsync();
        }
    }
}