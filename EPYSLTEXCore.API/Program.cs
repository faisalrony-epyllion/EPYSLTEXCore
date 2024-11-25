#region Using

using EPYSLTEXCore.API.CustomMiddlwares;
using EPYSLTEXCore.API.Extension;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using NLog.Extensions.Logging;
using NLog.Web;
using System.Text;
#endregion

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache(opt =>
{
    opt.SizeLimit = 100; // Set the caching key limit
});

builder.Services.AddApplication(); // Services LifeTime
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // Set session timeout
});

#region Swagger config
//builder.Services.AddSwaggerGen();
#endregion

#region AutoMapper
//builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
#endregion

#region NLogConfig
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    logging.AddNLog(builder.Configuration.GetSection("v"));
});
builder.Host.UseNLog();
#endregion

#region Authentication and Authorization

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
        options.SlidingExpiration = true;
    });

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var validateJwt = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
    ValidAudience = builder.Configuration["JwtSettings:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new ArgumentException("security key can not be null"))),
};

builder.Services.AddAuthentication().AddJwtBearer(jwt =>
{
    jwt.SaveToken = true;
    jwt.TokenValidationParameters = validateJwt;
});

builder.Services.AddSingleton(validateJwt);

#endregion

#region CORS Policy Configuration

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policyBuilder =>
    {
        policyBuilder.WithOrigins("https://localhost:44311") // Allow specific origin
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials(); // Include credentials if needed
    });
});

#endregion

var app = builder.Build();

#region Middleware Configuration

// Enable CORS
app.UseCors("AllowSpecificOrigin");

app.UseMiddleware<GlobalExceptionHandler>();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

#endregion

#region Routing

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

#endregion

app.Run();
