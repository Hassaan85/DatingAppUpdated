using API.Data;
using API.Extentions;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);
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
        });

var app = builder.Build();
app.UseCors(x=>x.AllowAnyHeader().AllowAnyMethod()
  .WithOrigins("http://localhost:4200","https://localhost:4200"));
  
// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// using var scope = app.Services.CreateScope();
// var services = scope.ServiceProvider;
 
//  try {
//    var context = services.GetRequiredService<DataContext>();
//    await context.Database.MigrateAsync();
//    await Seed.SeedUser(context);
//  }
//  catch (Exception ex) {
  
//    var logger = services.GetRequiredService<ILogger<Program>>();
//    logger.LogError(ex , "error occured");
 
//  }

app.Run();
