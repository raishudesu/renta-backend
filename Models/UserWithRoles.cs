namespace backend.Models;

public class UserWithRoles
{
    public User User { get; set; }
    public List<string> Roles { get; set; }
}