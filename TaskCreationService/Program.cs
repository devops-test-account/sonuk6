using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskCreationService.Data;
using TaskCreationService.Models;
using TaskCreationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<TaskService>();

// JWT Authentication Setup for Task Creation Service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Set to true if using HTTPS
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],   // JWT issuer (should be the same as the API Gateway)
            ValidAudience = builder.Configuration["Jwt:Audience"], // JWT audience (should be the same as the API Gateway)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])) // JWT key for signature validation
        };
    });

builder.Services.AddAuthorization();

// Add Swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Creation API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Creation API v1"));
}

// Use Authentication and Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Protect the API endpoints with JWT Authorization
app.MapGet("/api/task/{id}", [Authorize] (int id, TaskService taskService) =>
{
    var task = taskService.GetTaskById(id);
    return task != null ? Results.Ok(task) : Results.NotFound();
});

app.MapPost("/api/task", [Authorize] (TaskEntity task, TaskService taskService) =>
{
    taskService.CreateTask(task);
    return Results.Created($"/api/task/{task.Id}", task);
});

app.Run();
