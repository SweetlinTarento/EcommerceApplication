using EcommerceApplication.Data;
using EcommerceApplication.Data.Seeder;
using EcommerceApplication.Repository;
using EcommerceApplication.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EcommerceApplication.Models;
using EcommerceApplication.Services;
using EcommerceApplication.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Microsoft.OpenApi;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        path:"Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: null,   // keeps ALL days
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EcommerceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Version = "v1",
        Title = "Ecommerce API",
        Description = "An ASP.NET Core Web API for managing an ecommerce platform"
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token in the format:{your token}"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});


builder.Host.UseSerilog();


builder.Services.AddIdentity<ApplicationUser, IdentityRole>() //enables password hashing, user validation, and other identity features for
                                                              //ApplicationUser and IdentityRole, IdentityRole is a class that represents a role in
                                                              //the identity system, allowing you to manage user roles and permissions effectively and
                                                              //create AspNetRoles table in the database
    .AddEntityFrameworkStores<EcommerceContext>() //Store the users inside the database EcommerceContext using Entity Framework
    .AddDefaultTokenProviders();// Add default token providers for password reset, email confirmation, etc.
var secret = builder.Configuration["Jwt:Key"];
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options => //used to add authentication services to the application,
                                              //allowing it to authenticate users
                                              //In this case, it sets up JWT (JSON Web Token) authentication as the default scheme for both authentication
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;//to authenticate the user
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;//to reject no token or invalid token condition
})
.AddJwtBearer(options =>//the parameters to consider when validating the JWT token.
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,//validating who created the token
        ValidateAudience = true,//validating who the token is intended for
        ValidateLifetime = true,//validating the expiration time of the token
        ValidateIssuerSigningKey = true,//validating the signature of the token to ensure it hasn't been tampered with

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(secret))
    };
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())//app.Service is a global container containing all services of the app, CreateScope() is to create a temporary mini-container
{
    var services = scope.ServiceProvider;//getting the services from the provider
    try
    {
        var context = services.GetRequiredService<EcommerceContext>();

        // Apply any pending migrations
        context.Database.Migrate();

        // Now seed the roles after migrations are applied
        await RoleSeeder.Seed(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();
