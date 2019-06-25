using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Models;
using DatingApp.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace DatingApp.API.Controllers
{

[AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public AuthController(
            IConfiguration config, 
            IMapper mapper, 
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _config = config;
        }
        
        [HttpPost("register")]

        //Register([FromBody] string username, string password ), but we can use ViewModel to Viewing the properties which we want.
        public async Task<IActionResult> Register([FromBody] UserForRegisterViewModel userForRegisterViewModel)
        {
            //If we using that ApiController, we don't need to use model state validation or [FromBody]
            if (!ModelState.IsValid) return BadRequest(ModelState);
            //<Destination>(Source)
            var userToCreate = _mapper.Map<User>(userForRegisterViewModel);
            IdentityResult result = await _userManager.CreateAsync(userToCreate, userForRegisterViewModel.Password);
            var userToReturn = _mapper.Map<UserForDetailedViewModel>(userToCreate);
            if(result.Succeeded)
            {
                return CreatedAtRoute("GetUser", new { Controller = "Users", id = userToCreate.Id }, userToReturn);
            }

            return BadRequest(result.Errors);
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginViewModel userLoginViewModel)
        {

            User user = await _userManager.FindByNameAsync(userLoginViewModel.Username);
            SignInResult result = await _signInManager.CheckPasswordSignInAsync(user,userLoginViewModel.Password, false);
            if (!result.Succeeded) return Unauthorized();
            User appUser = await _userManager.Users.Include(p => p.Photos)
                .FirstOrDefaultAsync(u => u.NormalizedUserName == userLoginViewModel.Username.ToUpper());
            var userToReturn = _mapper.Map<UserForListViewModel>(appUser);

            return Ok(new
            {
                token =  GenerateJwtToken(appUser).Result,
                user = userToReturn
            });
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
            
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
            
            };
            //Adding user roles to the token.
            IList<string> roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
         
            
        }



    }
}