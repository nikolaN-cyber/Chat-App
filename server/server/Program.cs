using Core.Helpers;
using Core.Hubs;
using Core.Interfaces;
using Core.Services;
using Core.Workers;
using Infrastructure.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Prometheus;
using Core.Handlers;

var builder = WebApplication.CreateBuilder(args);

//Basic services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularClient",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

//Error handling services
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//Database config
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("Infrastructure")));


//Custom services
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();

//Email infrastructure
builder.Services.AddSingleton<IEmailQueue, EmailQueue>();
builder.Services.AddHostedService<EmailBackgroundWorker>();

//Filter logger
builder.Services.AddScoped<LogUserActivity>();

//Controllers registration
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<LogUserActivity>();
});

//Auth config
var jwtKey = builder.Configuration["jwt:key"] ?? throw new Exception("Jwt secret key not provided");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

//App building and middleware pipelines
var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseExceptionHandler();

app.UseCors("AllowAngularClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/hub");

app.MapControllers();

app.UseHttpMetrics();
app.MapMetrics();

app.Run();