using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class User : IdentityUser
    {

        // for one-to-one relation
        // public virtual UserTask UserTask { get; set; } = null!;

        // for one-to-many relation
        // public virtual ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();

    }
}