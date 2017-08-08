using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CaptionThis.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [DataType(DataType.Text)]
        [Display(Name = "API Token")]
        public string ApiToken { get; set; }
    }
}
