using CatTok.Api.Exceptions;
using CatTok.Api.Extensions;
using CatTok.Application.Commands.Login;
using CatTok.Common.Options;
using CatTok.Domain.Entities;
using CatTok.Infrastructure.Services;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CatTok.Api.Endpoints.Auth;

public class HandleRegister : IModule
{
    public class Request
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        
        public class Validator : AbstractValidator<Request>
        {
            public Validator()
            {
                RuleFor(b => b.Username).NotEmpty();
                RuleFor(b => b.Email).NotEmpty();
                RuleFor(b => b.Password).NotEmpty();
            }
        }
    }
    
    public async Task<IResult> Handle(
        HttpContext httpContext,
        [FromServices] ISender sender,
        [FromServices] IOptions<JwtOptions> jwtOptions,
        [FromServices] JwtService jwtService,
        [FromBody] Request? request,
        [FromServices] IValidator<Request> validator)
    {
        await ApiException.ValidateAsync(validator, request);

        var (response, refreshToken) = await sender.Send(request.Adapt<RegisterUserRequest>());

        if (response.IsError)
        {
            var type = response.FirstError.Type;
            return CustomResults.ErrorJson(type, response.Errors);
        }
        
        httpContext.Response.Cookies.Append("refresh-token", refreshToken, new()
        {
            Expires = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpiryTimeInDays),
            Domain = httpContext.Request.Host.Host,
            Path = "/",
            Secure = true
        });
        
        var user = new User
        {
            Id = response.Value.Id,
            Email = response.Value.Email,
            Picture = null,
            Sub = null,
            Username = response.Value.Username,
            PasswordHash = jwtService.Hash(request!.Password!),
            IsGoogleUser = false,
        };
        
        var token = jwtService.GenerateToken(user);

        return Results.Ok(new
        {
            access_token = token,
            user_info = response.Value
        });
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/register", Handle);
        return endpoints;
    }
}