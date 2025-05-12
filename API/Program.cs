using API.Data;
using API.Extentions;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using API.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityCore<AppUser>(
  opt => {
           opt.Password.RequireNonAlphanumeric=false;
  })
      .AddRoles<AppRole>()
      .AddRoleManager<RoleManager<AppRole>>()
      .AddEntityFrameworkStores<DataContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options => {
          var tokenKey = builder.Configuration["TokenKey"] ?? throw new Exception("Token Key Not Found");
          options.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
          };

           options.Events = new JwtBearerEvents
           {
              OnMessageReceived = context => 
              {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"));
                {
                  context.Token = accessToken;
                }
                return Task.CompletedTask;
              }
           };
        });
  builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdminRole" , policy => policy.RequireRole("Admin"))
        .AddPolicy("ModeratePhotoRole" , policy => policy.RequireRole("Admin" , "Moderator"));


var app = builder.Build();
app.UseCors(x=>x.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
  .WithOrigins("http://localhost:4200","https://localhost:4200"));
  
// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
 
 try {
   var context = services.GetRequiredService<DataContext>();
   var userManager =  services.GetRequiredService<UserManager<AppUser>>();
   var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
   await context.Database.MigrateAsync();
   await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
   await Seed.SeedUser(userManager , roleManager);
 }
 catch (Exception ex) {
  
   var logger = services.GetRequiredService<ILogger<Program>>();
   logger.LogError(ex , "error occured");
 
 }

app.Run();
