#region Using

using EPYSLTEXCore.API.CustomMiddlwares;
using EPYSLTEXCore.Application.DataAccess;
using EPYSLTEXCore.Application.DataAccess.Interfaces;
using EPYSLTEXCore.Application.Interfaces;
using EPYSLTEXCore.Application.Services;
using EPYSLTEXCore.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NLog.Extensions.Logging;
using NLog.Web;
using System.Text;
#endregion

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
//builder.Services..AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache(opt => { ////for in memory caching
    opt.SizeLimit = 100; ///// Set the caching key limit
});

#region Swagger config
//builder.Services.AddSwaggerGen();

#endregion

#region Services LifeTime
//builder.Services.AddSingleton<DapperDBContext>();

// Register IDapperCRUDService<T> with DapperCRUDService<T>
builder.Services.AddScoped(typeof(IDapperCRUDService<>), typeof(DapperCRUDService<>));
builder.Services.AddTransient<IMenuService, MenuService>();
builder.Services.AddTransient<IMenuDAL, MenuDAL>();
builder.Services.AddTransient<IReportAPISetupService, ReportAPISetupService>();
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

#region Authentification
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var validateJwt = new TokenValidationParameters
{
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new ArgumentException("security key can not be null"))),
};


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(jwt =>
{

    jwt.SaveToken = true;
    jwt.TokenValidationParameters = validateJwt;
});
builder.Services.AddSingleton(validateJwt);
#endregion

#region Cors
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("RequestPipeline",
//        builder =>
//        {
//            if (builder == null) throw new ArgumentNullException(nameof(builder));
//            builder.WithOrigins()
//                   .AllowAnyHeader()
//                   .AllowAnyMethod();
//        });
//});
#endregion

var app = builder.Build();

#region Use Swagger
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    //app.UseSwagger();
    //app.UseSwaggerUI(c =>
    //{
    //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Epyllion Group Expense Management System API v1");
    //});
}
#endregion


app.UseHttpsRedirection();
//app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
#region Custom Middlwares
app.UseMiddleware<GlobalExceptionHandler>();
//app.UseMiddleware<LoggingMiddleware>();
//app.UseMiddleware<RateLimitingMiddleware>();
#endregion

//app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

//app.UseCors("RequestPipeline");
app.Run();
