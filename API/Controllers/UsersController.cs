using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers

{
    public class UsersController(IUserRepository userRepository ) : BaseApiController
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
    }
}