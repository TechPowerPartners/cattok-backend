using CatTok.Common.Options;
using CatTok.Domain.Entities;
using CatTok.Infrastructure;
using CatTok.Infrastructure.Services;
using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.Extensions.Options;

namespace CatTok.Application.Commands.Login;

public record RegisterUserRequest : IRequest<(ErrorOr<RegisterUserResponse>, string)>
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public record RegisterUserResponse
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
}

public class RegisterUserRequestHandler(
    AppDbContext appDbContext,
    IOptions<JwtOptions> jwtOptions,
    JwtService jwtService)
    : IRequestHandler<RegisterUserRequest, (ErrorOr<RegisterUserResponse>, string)>
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly IOptions<JwtOptions> _jwtOptions = jwtOptions;
    private readonly JwtService _jwtService = jwtService;

    public async Task<(ErrorOr<RegisterUserResponse>, string)> Handle(RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var userExists = _appDbContext.Users.FirstOrDefault(u => u.Email == request.Email);

        if (userExists is not null)
            return (Error.Failure(description: "user with given email already exists"), "");

        var refreshToken = _jwtService.GenerateRefreshToken();

        var user = new User
        {
            Email = request.Email,
            Username = request.Username,
            Picture = null,
            Sub = null,
            IsGoogleUser = false,
            PasswordHash = _jwtService.Hash(request.Password),
            RefreshToken = _jwtService.Hash(refreshToken),
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtOptions.Value.RefreshTokenExpiryTimeInDays)
        };

        await _appDbContext.Users.AddAsync(user, cancellationToken);
        await _appDbContext.SaveChangesAsync(cancellationToken);

        return (user.Adapt<RegisterUserResponse>(), refreshToken);
    }
}