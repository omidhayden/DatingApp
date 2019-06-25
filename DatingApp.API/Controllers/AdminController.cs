using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using DatingApp.API.ViewModels;
using Microsoft.AspNetCore.Identity;
using DatingApp.API.Models;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserManager<User> _userManager;
        public AdminController(DataContext context, UserManager<User> userManager)
        {
            _userManager = userManager;

            _context = context;

        }

        [Authorize(Policy = "AdminRole")]
        [HttpGet("usersWithRoles")]
        public async Task<IActionResult> GetUsersWithRoles()
        {
            //Using Linq highly recomended for many to many relationships
            //For using Linq you need to add manually system.linq 
            var userList = await (_context.Users
                .Select(user => new
                {
                    user.Id,
                    user.UserName,
                    Roles = (from userRole in user.UserRoles
                        join role in _context.Roles on userRole.RoleId equals role.Id
                        select role.Name).ToList()
                })
                .OrderBy(user => user.UserName)).ToListAsync();
            return Ok(userList);
        }


        [Authorize(Policy = "AdminRole")]
        [HttpPost("editRoles/{userName}")]
        public async Task<IActionResult> EditRoles(string userName, RoleEditViewModel roleEditViewModel)
        {
            User user = await _userManager.FindByNameAsync(userName);
            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            string[] selectedRoles = roleEditViewModel.RoleNames;

            selectedRoles = selectedRoles ?? new string[] {};

            IdentityResult result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if(!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if(!result.Succeeded) return BadRequest("Failed to remove the roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }
        

        [Authorize(Policy = "PhotoRole")]
        [HttpGet("photosForModeration")]
        public IActionResult GetPhotosForModeration()
        {
            return Ok("Admins or moderators can see this");
        }

    }
}