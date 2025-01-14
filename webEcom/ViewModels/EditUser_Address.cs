using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using webEcom.Models;

namespace webEcom.ViewModels
{
	public class EditUser_Address
	{
		[Required(ErrorMessage = "Enter your new address")]
		public string? UserAddress { get; set; }
	}
}
