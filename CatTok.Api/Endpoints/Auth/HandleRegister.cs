﻿using CatTok.Api.Exceptions;
using CatTok.Api.Extensions;
using CatTok.Api.Services;
using CatTok.Application.Commands.Login;
using CatTok.Common.Options;
using CatTok.Infrastructure.Services;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CatTok.Api.Endpoints.Auth;

public class HandleRegister : IModule
{
    public class RegisterRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        
        public class Validator : AbstractValidator<RegisterRequest>
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
        [FromServices] CookieService cookieService,
        [FromServices] JwtService jwtService,
        [FromBody] RegisterRequest? request,
        [FromServices] IValidator<RegisterRequest> validator)
    {
        await ApiException.ValidateAsync(validator, request);

        var (response, refreshToken, accessToken) = await sender.Send(request.Adapt<RegisterUserRequest>());

        if (response.IsError)
        {
            var type = response.FirstError.Type;
            return CustomResults.ErrorJson(type, response.Errors);
        }

        cookieService.RegisterRefreshTokenCookie(httpContext, refreshToken);

        return Results.Ok(new
        {
            access_token = accessToken,
            user_info = response.Value
        });
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/register", Handle);
        return endpoints;
    }
}