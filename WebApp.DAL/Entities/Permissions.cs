namespace WebApp.DAL.Entities
{
    [Flags]
    public enum Permissions
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4
    }
}