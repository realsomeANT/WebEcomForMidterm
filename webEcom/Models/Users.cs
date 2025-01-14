using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace webEcom.Models
{
    public class Users : IdentityUser
    {
        public string? UserSurname { get; internal set; }
        public string? UserConfirmPassword { get; internal set; }
        public int UserPhoneNumber { get; internal set; }
        public string? UserAddress { get; internal set; }
        public string? UserPassword { get; internal set; }
        [DisplayName("UserFirstName")]
        public string? UserFirstName { get; internal set; }
        [DefaultValue("User")]
        public string? UserCategory { get; internal set; }
        public byte[]? UserProfile { get; internal set; }
    }
}
