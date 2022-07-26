using ExternalLoginsApp.Data;
using ExternalLoginsApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle 
var config = builder.Configuration;

builder.Services.AddDbContext<ExternalLoginContext>(option => option
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); 

builder.Services.AddIdentity<User, IdentityRole>(opt => {
        opt.Password.RequireUppercase = false;
        opt.Password.RequiredLength = 5;
    })
    .AddEntityFrameworkStores<ExternalLoginContext>();

builder.Services.AddAuthentication()
    .AddGoogle(options => {
        options.ClientId = config["ExternalLogins:GoogleAuth:ClientId"];
        options.ClientSecret = config["ExternalLogins:GoogleAuth:ClientSecret"];
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
