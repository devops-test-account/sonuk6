using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using UserManagementService.Data;
using UserManagementService.Models;
using UserManagementService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<UserService>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings.Key);

// JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization();

// Swagger configuration with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management API", Version = "v1" });

    // Add security definition for JWT token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer in the format **Bearer {token}**",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    // Apply the security definition globally to all endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Management API v1");
        c.DocumentTitle = "User Management API";

        // Enable the Swagger UI to request Authorization header for all requests
        c.OAuthClientId("swagger");
        c.OAuthAppName("Swagger UI Authentication");
    });
}

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Login endpoint to generate JWT
app.MapPost("/api/login", (UserEntity login, UserDbContext dbContext, IOptions<JwtSettings> jwtOptions) =>
{
    var user = dbContext.Users.SingleOrDefault(u => u.UserName == login.UserName && u.Password == login.Password);
    if (user == null)
        return Results.Unauthorized();

    var jwt = jwtOptions.Value;
    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(jwt.Key);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("UserId", user.Id.ToString())
        }),
        Expires = DateTime.UtcNow.AddMinutes(jwt.ExpiryMinutes),
        Issuer = jwt.Issuer,
        Audience = jwt.Audience,
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new { Token = tokenString });
});

// Protected user endpoint
app.MapGet("/api/user/{id}", [Authorize] (int id, UserService userService) =>
{
    var user = userService.GetUserById(id);
    return user != null ? Results.Ok(user) : Results.NotFound();
});

// Protected user creation endpoint
app.MapPost("/api/user", [Authorize] (UserEntity user, UserService userService) =>
{
    userService.CreateUser(user);
    return Results.Created($"/api/user/{user.Id}", user);
});

app.Run();
