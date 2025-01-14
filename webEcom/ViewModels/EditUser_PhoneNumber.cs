using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using webEcom.Models;

namespace webEcom.ViewModels
{
	public class EditUser_PhoneNumber
	{
		[Required(ErrorMessage = "Enter your new phone number")]
		public int UserPhoneNumber { get; set; }
	}
}
