using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers

{
  [Authorize]
      public class UsersController(IUserRepository userRepository , IMapper mapper) : BaseApiController
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
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (username == null) return BadRequest ("No username found in token");

            var user = await userRepository.GetUserByUserNameAsync(username);

            if (user == null) return BadRequest ("Could not find User");

            mapper.Map(memberUpdateDto , user);

            userRepository.Update(user);

            if (await userRepository.SaveAllAsync()) return  NoContent();

            return BadRequest ("failed to update User");
        }
    }
}