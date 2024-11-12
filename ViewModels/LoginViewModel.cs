using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Yot_Login2.Models;

namespace Yot_Login2.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Enter your email")]
        [EmailAddress]
        [Display(Name = "Email")] 
        public string? UserEmail { get; set; }
        [Required(ErrorMessage = "Enter your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")] 
        public string? UserPassword { get; set; }
        public bool RememberMe { get; set; }
    }
}
