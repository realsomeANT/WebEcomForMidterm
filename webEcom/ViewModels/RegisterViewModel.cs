using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace webEcom.ViewModels
{
    public class RegisterViewModel
    {
        [Key]
        public int IdUser { get; set; }
        [Required(ErrorMessage = "Enter your email")]
        [EmailAddress]
        public string? UserEmail { get; set; }
        [Required(ErrorMessage = "Enter your password")]
        [PasswordPropertyText]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "MinimumLength must more then 8")]
        [DataType(DataType.Password)]
        [Compare("UserConfirmPassword", ErrorMessage = "Passwords do not match")]
        [RegularExpression(@"(?=.*\d).{8,40}", ErrorMessage = "Password must contain at least one digit")]
        public string? UserPassword { get; set; }
        [Required(ErrorMessage = "Enter your password again")]
        [PasswordPropertyText]
        [DataType(DataType.Password)]
        public string? UserConfirmPassword { get; set; }
        [DefaultValue("User")]
        public string? UserCategory { get; set; }



        [Required(ErrorMessage = "Enter your name")]
        public string? UserFirstName { get; set; }
        [Required(ErrorMessage = "Enter your surname")]
        public string? UserSurname { get; set; }
        [Required(ErrorMessage = "Enter your address")]
        public string? UserAddress { get; set; }
        [Required(ErrorMessage = "Enter your phone number")]
        public int UserPhoneNumber { get; set; }
        public byte[]? UserProfile { get; set; }
        [NotMapped]
        public IFormFile? UserProfile_IFormFile { get; set; }
    }
}
