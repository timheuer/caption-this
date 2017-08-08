using System.ComponentModel.DataAnnotations;

namespace CaptionThis.Models.ManageViewModels
{
    public class SetApiTokenViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "API Token")]
        public string ApiToken { get; set; }
    }
}
