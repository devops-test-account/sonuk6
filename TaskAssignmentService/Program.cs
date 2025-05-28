using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskAssignmentService.Data;
using TaskAssignmentService.Model;
using TaskAssignmentService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TaskAssignmentManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<MessagePublisher>();
builder.Services.AddScoped<TaskAssignmentManagementService>();

// JWT Authentication Setup for Task Assignment Service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Set to true if using HTTPS
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],   // JWT issuer (should match with API Gateway)
            ValidAudience = builder.Configuration["Jwt:Audience"], // JWT audience (should match with API Gateway)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"])) // JWT key for signature validation
        };
    });

builder.Services.AddAuthorization();

// Add Swagger support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Task Assignment Management API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Assignment Management API v1"));
}

// Use Authentication and Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Protect the API endpoints with JWT Authorization
app.MapGet("/api/taskassignment/{id}", [Authorize] (int id, TaskAssignmentManagementService taskAssignmentService) =>
{
    var taskAssignment = taskAssignmentService.GetTaskAssignmentById(id);
    return taskAssignment != null ? Results.Ok(taskAssignment) : Results.NotFound();
});

app.MapPost("/api/taskassignment", [Authorize] (TaskAssignmentManagementEntity taskAssignment, TaskAssignmentManagementService taskAssignmentService) =>
{
    taskAssignmentService.AssignTask(taskAssignment);
    return Results.Created($"/api/taskassignment/{taskAssignment.Id}", taskAssignment);
});

app.Run();
