using WebApp.DAL.Entities;

namespace WebApp.UI.ViewModels
{
    public class AssignUsersBulkViewModel
    {
        public List<TaskAssignment> TaskAssignments { get; set; } = [];
        public List<DAL.Entities.Task> Tasks { get; set; } = [];
        public List<User> Users { get; set; } = [];
    }

    public class TaskAssignment
    {
        public int TaskId { get; set; }
        public List<int> SelectedUserIds { get; set; } = [];
    }
}
