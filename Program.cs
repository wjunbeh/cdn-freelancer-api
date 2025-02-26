using Microsoft.EntityFrameworkCore;
using CdnFreelancerApi.Data;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

// Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI();
    
// Configure middleware
app.UseHttpsRedirection();
app.UseAuthorization(); // Ensure authentication is enabled if required

// Ensure API controllers are mapped
app.MapControllers();

app.Run();