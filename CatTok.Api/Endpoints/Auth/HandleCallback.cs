using CatTok.Api.Extensions;
using CatTok.Application.Commands.GoogleLogin;
using CatTok.Common.Options;
using CatTok.Domain.Entities;
using CatTok.Infrastructure.Services;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CatTok.Api.Endpoints.Auth;

public class HandleCallback : IModule
{
    public async Task<IResult> HandleAsync(
        [FromServices] ExternalAuthService externalAuthService,
        [FromServices] IOptions<JwtOptions> jwtOptions,
        [FromServices] JwtService jwtService,
        [FromServices] ISender sender,
        HttpContext httpContext,
        [FromQuery] string code)
    {
        var callback = $"https://{httpContext.Request.Host}{httpContext.Request.Path}";
        var tokens = await externalAuthService.GetCredentials(code, callback);

        if (tokens.IsError)
        {
            return CustomResults.ErrorJson(400, tokens.Errors);
        }

        var userInfo = await externalAuthService.GetUserInfo(tokens.Value.AccessToken);

        var user = new User
        {
            Email = userInfo.Email,
            Picture = userInfo.Picture,
            Sub = userInfo.Id,
            Username = userInfo.Name,
            PasswordHash = null,
            IsGoogleUser = true,
        };

        var (userResponse, refreshToken) = await sender.Send(user.Adapt<GoogleLoginUserRequest>());

        user.Id = userResponse.Id;

        httpContext.Response.Cookies.Append("refresh-token", refreshToken, new()
        {
            Expires = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpiryTimeInDays),
            Domain = httpContext.Request.Host.Host,
            Path = "/",
            Secure = true
        });

        var token = jwtService.GenerateToken(user);

        return Results.Ok(new
        {
            access_token = token,
            user_info = userResponse
        });
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/auth/google/callback", HandleAsync);
        return endpoints;
    }
}