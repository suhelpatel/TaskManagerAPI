// =========================
// USING STATEMENTS
// =========================

// Handles JWT authentication middleware
using Microsoft.AspNetCore.Authentication.JwtBearer;

// Entity Framework Core (database operations)
using Microsoft.EntityFrameworkCore;

// Used for validating JWT tokens (security)
using Microsoft.IdentityModel.Tokens;

// Swagger / OpenAPI models
using Microsoft.OpenApi.Models;

using System.Text;

// Your project-specific namespaces
using TaskManagerAPI.Data;
using TaskManagerAPI.Interfaces;
using TaskManagerAPI.Services;


// =========================
// CREATE BUILDER
// =========================

var builder = WebApplication.CreateBuilder(args);

// 👉 This creates the app builder which:
// - Reads configuration (appsettings.json)
// - Registers services (Dependency Injection)
// - Prepares middleware pipeline


// =========================
// ADD SERVICES TO CONTAINER
// =========================

// Enables API Controllers (IMPORTANT)
// Without this, your controllers won't work
builder.Services.AddControllers();

// Enables endpoint discovery for Swagger
builder.Services.AddEndpointsApiExplorer();


// =========================
// SWAGGER CONFIGURATION
// =========================

// Swagger = API documentation + testing UI
builder.Services.AddSwaggerGen(options =>
{
    // Basic API info (shown in Swagger UI)
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Manager API",
        Version = "v1"
    });

    // 🔐 This tells Swagger:
    // "We are using JWT Authentication"
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",              // Header name
        Type = SecuritySchemeType.Http,      // HTTP authentication
        Scheme = "bearer",                  // Must be "bearer"
        BearerFormat = "JWT",               // Format of token
        In = ParameterLocation.Header,      // Token goes in header
        Description = "Enter: Bearer YOUR_TOKEN"
    });

    // 🔐 This enforces security globally in Swagger
    // So every API will require token (unless marked public)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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


// =========================
// DEPENDENCY INJECTION (DI)
// =========================

// Registers your service with DI container
// Meaning:
// Whenever ITaskService is needed → give TaskService
builder.Services.AddScoped<ITaskService, TaskService>();

// 👉 Scoped = new instance per request
// (Best for web APIs)


// =========================
// DATABASE CONFIGURATION
// =========================

// Connects your app to SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// 👉 Reads connection string from appsettings.json
// 👉 Enables EF Core (ORM) to talk to database


// =========================
// JWT AUTHENTICATION SETUP
// =========================

// Read secret key from config
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

// Add Authentication service
builder.Services.AddAuthentication(options =>
{
    // Set JWT as default authentication scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Disable HTTPS requirement (OK for development only)
    options.RequireHttpsMetadata = false;

    // Save token in HttpContext
    options.SaveToken = true;

    // Token validation rules (VERY IMPORTANT)
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,   // Check if key is valid
        ValidateIssuer = false,            // Skip issuer validation (for now)
        ValidateAudience = false,          // Skip audience validation (for now)

        // This is the secret key used to sign token
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});


// =========================
// BUILD APP
// =========================

var app = builder.Build();

// 👉 This compiles everything into a runnable app


// =========================
// MIDDLEWARE PIPELINE
// =========================

// Only enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();       // Generates Swagger JSON
    app.UseSwaggerUI();     // Provides UI (browser view)
}


// Forces HTTPS (security best practice)
app.UseHttpsRedirection();


// 🔥 VERY IMPORTANT ORDER

// Step 1: Identify WHO the user is
app.UseAuthentication();

// Step 2: Check WHAT the user is allowed to do
app.UseAuthorization();


// Maps controller routes (activates APIs)
app.MapControllers();


// =========================
// RUN APPLICATION
// =========================

app.Run();

// 👉 Starts the web server (Kestrel)