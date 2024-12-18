namespace WebApp.DAL.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<Role> Roles { get; set; } = [];
        public List<Task> Tasks { get; set; } = []; 
    }
}