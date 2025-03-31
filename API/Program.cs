using API.Data;
using API.Extentions;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();
app.UseCors(x=>x.AllowAnyHeader().AllowAnyMethod()
  .WithOrigins("http://localhost:4200","https://localhost:4200"));
  
// Configure the HTTP request pipeline.
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
