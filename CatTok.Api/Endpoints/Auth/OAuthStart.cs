using CatTok.Infrastructure.Services;
using FastEndpoints;

namespace CatTok.Api.Endpoints.Auth;

public class OAuthStart(ExternalAuthService externalAuthService) : EndpointWithoutRequest
{
    private readonly ExternalAuthService _externalAuthService = externalAuthService;

    public override void Configure()
    {
        Get("/api/auth/google/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var callback = $"https://{HttpContext.Request.Host}/api/auth/google/login";
        var uri = _externalAuthService.GetGoogleAuthUrl(callback);
        await SendRedirectAsync(uri, allowRemoteRedirects: true);
    }
}