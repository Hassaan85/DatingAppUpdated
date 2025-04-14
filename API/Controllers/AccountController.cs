using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace API.Controllers
{
    public class AccountController (UserManager<AppUser> userManager , ITokenService tokenService) : BaseApiController
    {
        [HttpPost ("register")]

        public async Task<ActionResult<UserDto>> Register ( RegisterDto registerDto )
        {
            if (await UserExists(registerDto.UserName)) return  BadRequest ( "UserExists" );

            return Ok ();
            // var result = await userManager.CreateAsync(user , registerDto.Password);

            // if (!result.Succeeded) return BadRequest ( result.Errors);


            // using var hmac = new HMACSHA512();

            // var user = new AppUser
            // {
            //     UserName =  registerDto.UserName.ToLower(),
            //     PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            //     PasswordSalt = hmac.Key
            // };
             

            // context.Users.Add (user);
            // await context.SaveChangesAsync ();

            // return new UserDto
            // {
            //     Username = user.UserName,
            //     Token  = tokenService.CreateToken(user)
            // };

            
        }

         [HttpPost ("login")]
         public async Task <ActionResult<UserDto>> Login ( LoginDto loginDto ) 
         {
           var user = await userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync (x => x.NormalizedUserName == loginDto.UserName.ToUpper());

           if  (user == null || user.UserName == null) return Unauthorized ("Invalid username");

           var result = await userManager.CheckPasswordAsync(user , loginDto.Password);

           if (!result ) return Unauthorized();

          //  using var hmac = new HMACSHA512(user.PasswordSalt);

          //  var ComputedHash = hmac.ComputeHash (Encoding.UTF8.GetBytes(loginDto.Password));

          //  for (int i = 0; i < ComputedHash.Length; i++)
          //  {
          //    if (ComputedHash[i] != user.PasswordHash[i]) return Unauthorized ("Invalid Password");
          //  }

           return new UserDto
           {
            Username= user.UserName,
            Token = await tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
            Gender = user.Gender

           };
         }

        private async Task<bool> UserExists (string username)
        {
            return await userManager.Users.AnyAsync(x => x.NormalizedUserName == username.ToUpper());
        }
        
    }
}