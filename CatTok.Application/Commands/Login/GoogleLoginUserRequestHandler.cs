using CatTok.Common.Options;
using CatTok.Domain.Entities;
using CatTok.Infrastructure;
using CatTok.Infrastructure.Services;
using Mapster;
using MediatR;
using Microsoft.Extensions.Options;

namespace CatTok.Application.Commands.Login;

public record GoogleLoginUserRequest : IRequest<(RegisterUserResponse, string)>
{
    public required string Sub { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Picture { get; set; }
}

public record GoogleLoginUserResponse
{
    public required Guid Id { get; set; }
    public required string Sub { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Picture { get; set; }
}

public class GoogleLoginUserRequestHandler(AppDbContext appDbContext, IOptions<JwtOptions> jwtOptions, JwtService jwtService)
    : IRequestHandler<GoogleLoginUserRequest, (RegisterUserResponse, string)>
{
    private readonly AppDbContext _appDbContext = appDbContext;
    private readonly IOptions<JwtOptions> _jwtOptions = jwtOptions;
    private readonly JwtService _jwtService = jwtService;

    public async Task<(RegisterUserResponse, string)> Handle(GoogleLoginUserRequest request, CancellationToken cancellationToken)
    {
        var user = _appDbContext.Users.FirstOrDefault(u => u.Sub == request.Sub && u.IsGoogleUser);
        
        if (user is null)
        {
            user = request.Adapt<User>();
            user.Id = Guid.NewGuid();
            await _appDbContext.Users.AddAsync(user, cancellationToken);
        }
        else
        {
            user.Username = request.Username;
            user.Email = request.Email;
            user.Picture = request.Picture;
        }
        
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = _jwtService.Hash(refreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtOptions.Value.RefreshTokenExpiryTimeInDays);
        user.IsGoogleUser = true;
        
        await _appDbContext.SaveChangesAsync(cancellationToken);
        return (user.Adapt<RegisterUserResponse>(), refreshToken);
    }
}
