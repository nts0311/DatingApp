using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly ILikesRepository likesRepository;
        private readonly IUserRepository userRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            this.userRepository = userRepository;
            this.likesRepository = likesRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var sourceUser = await likesRepository.GetUserWithLikes(sourceUserId);
            var likedUser = await userRepository.GetUserByUsernameAsync(username);

            if(likedUser == null) return BadRequest("User does not exsit.");

            var userLike = await likesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if(userLike != null) return BadRequest("Already liked this user.");

            userLike = new UserLike{
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if(await userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Unknown error.");
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetUserLikes([FromQuery]LikeParams likeParams)
        {
            likeParams.UserId = User.GetUserId();
            var users = await likesRepository.GetUserLikes(likeParams);

            Response.AddPaginationHeader(users.CurrentPage, 
                users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
    }
}