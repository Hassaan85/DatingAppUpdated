using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extentions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class LikesController(ILikesRepository likesRepository) : BaseApiController
    {
        [HttpPost("{targetUserId:int}")]

        public async Task<ActionResult>ToggleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();

            if (sourceUserId == targetUserId) return BadRequest("You cannot Like yourself");

            var existingLike = await likesRepository.GetUserLike(sourceUserId ,targetUserId);
            if (existingLike == null)
            {
                var like = new UserLike
                {
                       SourceUserId = sourceUserId,
                       TargetUserId = targetUserId
                };

                likesRepository.AddLike(like);
            }

            else 
            {
                likesRepository.DeleteLike(existingLike);
            }

            if (await likesRepository.SaveChanegs()) return Ok();

            return BadRequest("failed to update");

        }

        [HttpGet("list")]

        public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeIds()
        {
            return Ok (await likesRepository.GetCurrentUserLikeIds(User.GetUserId()));
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]LikedParams likedParams)
        {
            likedParams.UserId = User.GetUserId();
            var users = await likesRepository.GetUserLikes(likedParams);

            Response.AddPaginationHeader(users);
            return Ok(users);
        }
    }
}