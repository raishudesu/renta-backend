using backend.Data;
using Microsoft.EntityFrameworkCore;
using backend.Models;
using Microsoft.AspNetCore.Identity;

namespace backend.Services;

public class UserService
{
    private readonly AppDbContext db;
    // private readonly RoleManager<IdentityRole> _roleManager;

    private readonly UserManager<User> _userManager;


    public UserService(AppDbContext context, UserManager<User> userManager)
    {
        db = context;
        _userManager = userManager;
    }

    public async Task<List<UserWithRoles>> GetUsers()
    {
        try
        {
            var usersWithRoles = new List<UserWithRoles>();

            var users = await db.Users.Select(u => new User
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PasswordHash = null
            }).ToListAsync();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRoles
                {
                    User = user,
                    Roles = roles.ToList()
                });
            }

            return usersWithRoles;
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving users", ex);
        }
    }

    public async Task<User?> GetUserById(string id)
    {
        try
        {
            var user = await db.Users.FindAsync(id);


            if (user == null)
            {
                return null;
            }


            // return null for the password
            return new User
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PasswordHash = null
            };
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving user", ex);
        }
    }

    // public async Task<User?> GetUserWithTasks(string id)
    // {
    //     var user = await db.Users.Include(u => u.UserTasks).FirstOrDefaultAsync(u => u.Id == id);

    //     return user;
    // }

}