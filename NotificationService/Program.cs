using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using NotificationService.Data;
using NotificationService.Models;
using NotificationService.Services;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<NotificationManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHostedService<MessageConsumer>();
builder.Services.AddScoped<NotificationManagementService>();

// JWT Authentication Setup for Notification Service
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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Notification Management API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Management API v1"));
}

// Use Authentication and Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Protect the API endpoints with JWT Authorization
app.MapGet("/api/notification/{id}", [Authorize] (int id, NotificationManagementService notificationService) =>
{
    var notification = notificationService.GetNotificationById(id);
    return notification != null ? Results.Ok(notification) : Results.NotFound();
});

app.MapPost("/api/notification", [Authorize] (NotificationManagementEntity notification, NotificationManagementService notificationService) =>
{
    notificationService.SendNotification(notification);
    return Results.Created($"/api/notification/{notification.Id}", notification);
});

app.Run();
