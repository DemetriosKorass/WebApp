namespace WebApp.DAL.Entities
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<User>? Users { get; private set; } = []; 
    }
}