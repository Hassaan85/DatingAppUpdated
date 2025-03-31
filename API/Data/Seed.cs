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

namespace API.Data
{
    public class Seed
    {
        public static async Task SeedUser(DataContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var userData  = await File.ReadAllTextAsync("Data/UserSeedData.json");

            var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};

            var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

            if (users == null) return;

            foreach (var user in users)
            {
                using var hmac = new HMACSHA512();

                user.UserName = user.UserName.ToLower();
                user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes ("Pa$$w0rd"));
                user.PasswordSalt = hmac.Key;

                context.Users.Add(user);

            }

            await context.SaveChangesAsync();
        }
    }
}