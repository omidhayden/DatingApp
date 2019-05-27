using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.ViewModels
{
    public class UserForLoginViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}