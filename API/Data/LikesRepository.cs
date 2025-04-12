using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository(DataContext context , IMapper mapper) : ILikesRepository
    {
        public void AddLike(UserLike like)
        {
            context.Likes.Add(like);
        }

        public void DeleteLike(UserLike like)
        {
            context.Likes.Remove(like);
        }

        public async Task<UserLike?> GetUserLike(int SourceUserId, int TargetUserId)
        {
                return await context.Likes.FindAsync (SourceUserId, TargetUserId);
        }


        public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId)
        {
      
            return await context.Likes
            .Where(x => x.SourceUserId == currentUserId)
            .Select(x => x.TargetUserId)
            .ToListAsync();
        
        }

        public async Task<PagedList<MemberDto>> GetUserLikes(LikedParams likedParams)
        {
            var likes = context.Likes.AsQueryable();
            IQueryable<MemberDto> query;

            switch (likedParams.Predicate) 
            {
                   case "liked":
                          query = likes 
                          .Where(x => x.SourceUserId == likedParams.UserId)
                          .Select(x => x.TargetUser)
                          .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                       break;

                    case "likedBy":
                            query = likes 
                          .Where(x => x.TargetUserId == likedParams.UserId)
                          .Select(x => x.SourceUser)
                          .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                  break;
                     default :
                       var  likeIds=  await GetCurrentUserLikeIds(likedParams.UserId);

                       query = likes
                        .Where(x => x.TargetUserId == likedParams.UserId && likeIds.Contains(x.SourceUserId))
                        .Select(x => x.SourceUser)
                        .ProjectTo<MemberDto>(mapper.ConfigurationProvider);
                    break;

            }

            return await PagedList<MemberDto>.CreateAsync(query,likedParams.PageNumber , likedParams.PageSize);
        }

        public async Task<bool> SaveChanegs()
        {
            return await context.SaveChangesAsync() > 0; 
        }

    

       
    }
}