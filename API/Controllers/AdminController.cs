using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IUnitOfWork uow;
        public AdminController(UserManager<AppUser> userManager, IUnitOfWork uow)
        {
            this.uow = uow;
            this.userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUserWithRoles()
        {
            var usersWithRole = await userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ar => ar.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();

            return Ok(usersWithRole);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(",");

            var user = await userManager.FindByNameAsync(username);
            if (user == null) return BadRequest("Could not find user");

            var userRoles = await userManager.GetRolesAsync(user);

            var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded) return BadRequest("Could not add user to roles.");

            result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) return BadRequest("Could not remove from roles");

            return Ok(await userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration()
        {
            var photos = await uow.PhotoRespository.GetUnapprovedPhotos();

            if(photos != null) return Ok(photos);

            return BadRequest("Error getting photo waiting for approval");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{id}")]
        public async Task<ActionResult> ApprovePhoto(int id)
        {
            var photo = await uow.PhotoRespository.GetPhotoById(id);

            if(photo == null)  return NotFound();

           photo.IsApproved = true;

            photo.IsMain = await uow.UserRepository.NeedMainPhoto(id);


            return (await uow.Complete()) ? Ok() : BadRequest("Error aprroving photo.");
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{id}")]
        public async Task<ActionResult> RejectPhoto(int id)
        {
            var photo = await uow.PhotoRespository.GetPhotoById(id);
            uow.PhotoRespository.RemovePhoto(photo);

            return (await uow.Complete()) ? Ok() : BadRequest("Error aprroving photo.");
        }
    }
}