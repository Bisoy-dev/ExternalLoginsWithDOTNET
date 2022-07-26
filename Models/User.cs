using Microsoft.AspNetCore.Identity;

namespace ExternalLoginsApp.Models;

public class User : IdentityUser
{
    public int Age { get; set; }
}