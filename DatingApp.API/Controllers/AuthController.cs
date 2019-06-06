using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Models;
using DatingApp.API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{


    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        public AuthController(IAuthRepository repo, IConfiguration config, IMapper mapper)
        {
            _mapper = mapper;
            _config = config;
            _repo = repo;

        }

        [HttpPost("register")]

        //Register([FromBody] string username, string password ), but we can use ViewModel to Viewing the properties which we want.
        public async Task<IActionResult> Register([FromBody] UserForRegisterViewModel userVM)
        {

            //If we using that ApiController, we don't need to use model state validation or [FromBody]
            if (!ModelState.IsValid) return BadRequest(ModelState);

            userVM.Username = userVM.Username.ToLower();
            if (await _repo.UserExists(userVM.Username))
            {
                return BadRequest("Username already axist");
            }
            //<Destination>(Source)
            var userToCreate = _mapper.Map<User>(userVM);

            var createdUser = await _repo.Register(userToCreate, userVM.Password);
            var userToReturn= _mapper.Map<UserForDetailedViewModel>(createdUser);

            return CreatedAtRoute("GetUser", new {Controller="Users", id = createdUser.Id},userToReturn);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserForLoginViewModel userLoginVM)
        {

            var userFromRepo = await _repo.Login(userLoginVM.Username.ToLower(), userLoginVM.Password);
            if (userFromRepo == null) return Unauthorized();
            //We are give user's information to the localhost. 
            //For our token we need two claims. User's id and username. 
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
            new Claim(ClaimTypes.Name, userFromRepo.Username)

             };
            //This token key will be hashed local and no one can see what is in it.

            //It just needs to read a Token key from appsettings.Json
            var key = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes(_config.GetSection("AppSettings:Token").Value));
            //It should be hash the key which we just made it.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            //It contains our claims, the expiry date of our token and signing creditionals
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            //It creates our token/ Token Handler module neede to create token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var user = _mapper.Map<UserForListViewModel>(userFromRepo);

        return Ok(new
        {
            token = tokenHandler.WriteToken(token),
            user
        });
        }





    }
}