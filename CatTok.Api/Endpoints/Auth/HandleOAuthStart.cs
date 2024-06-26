﻿using CatTok.Api.Extensions;
using CatTok.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatTok.Api.Endpoints.Auth;

public class HandleOAuthStart : IModule
{
    public IResult Handle(
        HttpContext httpContext,
        [FromServices] ExternalAuthService externalAuthService)
    {
        var callback = $"https://{httpContext.Request.Host}/api/auth/google/callback";
        var uri = externalAuthService.GetGoogleAuthUrl(callback);
        return Results.Redirect(uri);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/auth/google/login", Handle);
        return endpoints;
    }
}