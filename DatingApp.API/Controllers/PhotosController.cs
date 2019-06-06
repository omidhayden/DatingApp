using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using DatingApp.API.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
        IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo;

         Account acc = new Account(
             _cloudinaryConfig.Value.CloudName,
             _cloudinaryConfig.Value.ApiKey,
             _cloudinaryConfig.Value.ApiSecret
         );
         _cloudinary = new Cloudinary(acc);

        }   
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnViewModel>(photoFromRepo);
            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId,[FromForm] PhotoForCreationViewModel photoForCreationViewModel)
        {
            //Check if the user id of url match with userid of the token
            #region Check User match url id with token
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId);

            #endregion 
            #region create file, Upload to the cloud
            var file = photoForCreationViewModel.File;
            var uploadResult = new ImageUploadResult();
            if(file.Length > 0)
            {
                using (var stream= file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                        .Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    //Response from cloudinary
                    uploadResult = _cloudinary.Upload(uploadParams);
                }

            }
            #endregion 
            #region Push the url result into viewmodel and map into model
            photoForCreationViewModel.Url = uploadResult.Uri.ToString();
            photoForCreationViewModel.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationViewModel);

            if (!userFromRepo.Photos.Any(u => u.isMain)) photo.isMain = true;

            userFromRepo.Photos.Add(photo);

           
            #endregion
            if(await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnViewModel>(photo);
                return CreatedAtRoute("GetPhoto", new Photo{Id = photo.Id}, photoToReturn);
            }
            return BadRequest("Could not add the photo");
        }

        #region Set the main photo
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) 
            return Unauthorized();

            var user = await _repo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))
            return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);
            if(photoFromRepo.isMain) {
                return BadRequest("This is already main photo");
            }

            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);

            currentMainPhoto.isMain = false;

            photoFromRepo.isMain =true;

            if(await _repo.SaveAll())
            return NoContent();

            return BadRequest("Could not set photo to main");

        }
        #endregion

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if(userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) 
            return Unauthorized();

            var user = await _repo.GetUser(userId);

            if(!user.Photos.Any(p => p.Id == id)) return Unauthorized();

            var photoFromRepo= await _repo.GetPhoto(id);
            if(photoFromRepo.isMain) return BadRequest("You can't delete your main photo");

            if(photoFromRepo.PublicId != null)
            {

                var deleteparams = new DeletionParams(photoFromRepo.PublicId);

                var result = _cloudinary.Destroy(deleteparams);

                if(result.Result == "ok")
                {
                   _repo.Delete(photoFromRepo);
                }
            }
            if(photoFromRepo.PublicId == null)
            {
                _repo.Delete(photoFromRepo);
            }

           
            if(await _repo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to delete the photo!");
        }
    }
}