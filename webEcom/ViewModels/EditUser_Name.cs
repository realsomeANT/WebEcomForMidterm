using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using webEcom.Models;

namespace webEcom.ViewModels
{
	public class EditUser_Name
	{
		[Required(ErrorMessage = "Enter your new name")]
		public string? UserFirstName { get; set; }
	}
}
