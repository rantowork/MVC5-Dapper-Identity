using System.ComponentModel.DataAnnotations;

namespace DapperIdentity.Web.ViewModels
{
    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
