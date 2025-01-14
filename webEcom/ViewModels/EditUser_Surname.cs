using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using webEcom.Models;

namespace webEcom.ViewModels
{
	public class EditUser_Surname
	{
		[Required(ErrorMessage = "Enter your new surname")]
		public string? UserSurname { get; set; }
	}
}
