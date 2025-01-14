using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using webEcom.Models;

namespace webEcom.ViewModels
{
    public class EditUser_Profile
    {
        public byte[]? UserProfile { get; set; }
        [NotMapped]
        public IFormFile? UserProfile_IFormFile { get; set; }
    }
}
