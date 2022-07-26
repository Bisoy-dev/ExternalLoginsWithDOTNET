using ExternalLoginsApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExternalLoginsApp.Data;

public class ExternalLoginContext : IdentityDbContext<User>
{
    public ExternalLoginContext(DbContextOptions<ExternalLoginContext> options) : base(options){}
}