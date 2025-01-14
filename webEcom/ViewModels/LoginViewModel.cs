using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using webEcom.Models;

namespace webEcom.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Enter your email")]
        [EmailAddress]
        [Display(Name = "Email")] 
        public string? UserEmail { get; set; }
        [Required(ErrorMessage = "Enter your password")]
        [PasswordPropertyText]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "MinimumLength must more then 8")]
        [RegularExpression(@"(?=.*\d).{8,40}", ErrorMessage = "Password must contain at least one digit")]
        [DataType(DataType.Password)]
        public string? UserPassword { get; set; }
        public bool RememberMe { get; set; }
    }
}
