using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{
    public class TokenService (IConfiguration config , UserManager<AppUser> userManager) : ITokenService
    {
        public async Task<string> CreateToken(AppUser user)
        {
           var tokenKey = config["TokenKey"] ?? throw new Exception ("Cannot Access Token Key");
           if (tokenKey.Length<64) throw new Exception ("Your Token Key needs to be longer");
           var key = new SymmetricSecurityKey (Encoding.UTF8.GetBytes (tokenKey));

           var claims = new List<Claim> 
           {
             new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
             new (ClaimTypes.Name , user.UserName)
            
           };

           var roles = await userManager.GetRolesAsync(user);

           claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

           var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
           var tokenDescriptor = new SecurityTokenDescriptor
           {
            Subject =  new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
           };

           var tokenHandler = new JwtSecurityTokenHandler();
           var token = tokenHandler.CreateToken(tokenDescriptor);

           return tokenHandler.WriteToken (token);
        }
    }

    
}