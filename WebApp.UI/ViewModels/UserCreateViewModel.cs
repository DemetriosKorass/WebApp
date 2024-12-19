using System.ComponentModel.DataAnnotations;

namespace WebApp.UI.ViewModels
{
    public class UserCreateViewModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required.")]
        public int? SelectedRoleId { get; set; }

        public List<DAL.Entities.Role> Roles { get; set; } = [];
        public List<DAL.Entities.Task> AllTasks { get; set; } = [];
        public int[] SelectedTasks { get; set; } = [];
    }
}
