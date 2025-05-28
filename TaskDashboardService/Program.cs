using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskDashboardService.Data;
using TaskDashboardService.Models;
using TaskDashboardService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TaskDashboardManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<TaskDashboardManagementService>();

// JWT Authentication Setup for Task Dashboard Service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Set to true if using HTTPS
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

// Add Swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Dashboard Management API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Dashboard Management API v1"));
}

// Use Authentication and Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Protect the API endpoints with JWT Authorization
app.MapGet("/api/taskdashboard/{id}", [Authorize] (int id, TaskDashboardManagementService taskDashboardService) =>
{
    var task = taskDashboardService.GetTaskById(id);
    return task != null ? Results.Ok(task) : Results.NotFound();
});

app.MapPost("/api/taskdashboard/{id}/status", [Authorize] (TaskDashboardManagementEntity taskDashboard, TaskDashboardManagementService taskDashboardService) =>
{
    taskDashboardService.UpdateTaskStatus(taskDashboard);
    return Results.Created($"/api/taskdashboard/{taskDashboard.Id}", taskDashboard);
    //return Results.Ok();
});

app.Run();
