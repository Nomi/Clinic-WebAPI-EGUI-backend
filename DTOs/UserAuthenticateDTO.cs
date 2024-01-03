using System.ComponentModel.DataAnnotations;

namespace EGUI_Stage2.DTOs
{
    public class UserAuthenticateDTO
    {
        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }
}
