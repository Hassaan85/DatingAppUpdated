using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController(UserManager<AppUser> userManager) : BaseApiController
    {
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]

        public async Task<ActionResult> GetUsersWithRoles () 
        {
           var users = await userManager.Users
             .OrderBy(x=> x.UserName)
             .Select(x => new {
                x.Id,
                Username = x.UserName,
                Roles = x.UserRoles.Select(r => r.Role.Name).ToList() 
             }).ToListAsync();

             return Ok (users);
        }


        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult>EditRoles(string userName , string roles) 
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("U need to Select atleast one roll");

            var selectedRoles = roles.Split(',').ToArray();

            var user = await userManager.FindByNameAsync(userName);

            if (user == null) return BadRequest("user not found");

            var userRoles = await userManager.GetRolesAsync(user);

            var result = await userManager.AddToRolesAsync(user , selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add roles");

            result = await userManager.RemoveFromRolesAsync(user , selectedRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to from remove roles");

            return Ok (await userManager.GetRolesAsync(user));
        } 

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]

        public ActionResult GetPhotoForModeration () 
        {
            return Ok ("only admins and moderators");
        }
    }
}