using System;
using Microsoft.AspNetCore.Http;

namespace DatingApp.API.ViewModels
{
    public class PhotoForCreationViewModel
    {
        public string Url { get; set; }
        public IFormFile File { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public string PublicId { get; set; }

        public PhotoForCreationViewModel()
        {
            DateAdded = DateTime.Now;
        }
    }
}