using System.Text;
using CatTok.Api.Extensions;
using CatTok.Api.Middlewares;
using CatTok.Api.Services;
using CatTok.Application;
using CatTok.Common.Options;
using CatTok.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddTransient<CookieService>();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<GoogleOAuthOptions>(builder.Configuration.GetSection("GoogleOAuthOptions"));

builder.Services.AddHttpClient();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.RegisterModules();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,    
            ValidateLifetime = builder.Configuration.GetValue<bool>("JwtSettings:ValidateLifetime"),
            ValidateIssuer = builder.Configuration.GetValue<bool>("JwtSettings:ValidateIssuer"),
            ValidateAudience = builder.Configuration.GetValue<bool>("JwtSettings:ValidateAudience"),
            ValidIssuers = builder.Configuration.GetValue<string[]>("JwtSettings:ValidIssuers"),
            ValidAudiences = builder.Configuration.GetValue<string[]>("JwtSettings:ValidAudiences"),
            IssuerSigningKey = 
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCustomExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

var apiGroup = app.MapGroup("api");
apiGroup.MapEndpoints();

app.MapGet("test", () => "Auth is working!").RequireAuthorization();

app.Run();