namespace WebApp.DAL.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Permissions Permissions { get; set; }
    }
}
