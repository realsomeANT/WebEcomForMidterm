using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using webEcom.Models;

namespace webEcom.ViewModels
{
	public class EditUser_Email
	{
		[Required(ErrorMessage = "Enter your new email")]
		[EmailAddress]
		public string? Email { get; set; }
	}
}
