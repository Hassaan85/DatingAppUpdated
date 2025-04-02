using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers

{
  [Authorize]
      public class UsersController(IUserRepository userRepository , IMapper mapper , IPhotoService photoService) : BaseApiController
    {
     
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users =  await  userRepository.GetMembersAsync();

           // var userTOReturn  = mapper.Map<IEnumerable<MemberDto>>(users);

            return Ok (users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var user = await userRepository.GetMemberAsync(username);

            if  (user == null) return NotFound ();

          //  return mapper.Map<MemberDto>(user);

           return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            // var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            // if (username == null) return BadRequest ("No username found in token");

            var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());

            if (user == null) return BadRequest ("Could not find User");

            mapper.Map(memberUpdateDto , user);

            userRepository.Update(user);

            if (await userRepository.SaveAllAsync()) return  NoContent();

            return BadRequest ("failed to update User");
        }

        [HttpPost("add-photo")]
        public async Task <ActionResult<PhotoDto>>Addphoto(IFormFile file)
        {
               var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());

               if (user == null) return BadRequest ("Cannot Update user");

               var result = await photoService.AddPhotoAsync(file);

               if (result.Error != null) return BadRequest (result.Error.Message);

               var photo = new Photo
               {
                Url = result.SecureUrl.AbsoluteUri,
                publicId = result.PublicId
               };

               user.Photos.Add(photo);

               if (await userRepository.SaveAllAsync()) 
               return CreatedAtAction(nameof(GetUser),
               new {username=user.UserName}, mapper.Map<PhotoDto>(photo));

               return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId:int}")]
        public async Task <ActionResult> SetMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());

            if ( user == null) return BadRequest ("Could not find User");

            var photo  = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null || photo.IsMain ) return BadRequest ("Cannot Use this as main photo");

            var currentMain  = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain=true;

            if (await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Problem setting up with the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task <ActionResult> DeletePhoto(int photoId)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());

            if (user == null) return BadRequest ("User not found");

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null || photo.IsMain ) return BadRequest("photo cannot be deleted");

            if (photo.publicId != null)
            {
                var result = await photoService.DeletePhotoAsync(photo.publicId);
                if (result.Error != null) return BadRequest (result.Error);
            }
             
             user.Photos.Remove(photo);

             if (await userRepository.SaveAllAsync()) return Ok ();

             return BadRequest("Problem Deleting Photo");
        }
    
    }
}