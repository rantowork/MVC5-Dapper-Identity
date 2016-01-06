using System.ComponentModel.DataAnnotations;

namespace DapperIdentity.Web.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 4)]
        [Display(Name = "Nickname")]
        public string Nickname { get; set; }
    }
}
