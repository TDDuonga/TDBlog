using System.ComponentModel.DataAnnotations;

namespace TDBlog.ViewModels
{
    public class ResetPassword
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        [Required]
        public string? NewPassword { get; set; }
        [Required]
        [Compare(nameof(NewPassword))]
        public string? ConfirmPassword { get; set; }
    }
}
