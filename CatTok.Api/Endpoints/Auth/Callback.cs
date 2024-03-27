using CatTok.Infrastructure.Services;
using FastEndpoints;

namespace CatTok.Api.Endpoints.Auth;

public class CodeRequest
{
    public required string Code { get; set; }
}

public class Callback(ExternalAuthService externalAuthService) : Endpoint<CodeRequest>
{
    private readonly ExternalAuthService _externalAuthService = externalAuthService;
    
    public override void Configure()
    {
        Get("/api/auth/google/callback");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CodeRequest req, CancellationToken ct)
    {
        var callback = $"https://{HttpContext.Request.Host}{HttpContext.Request.Path}";
        var tokens = await _externalAuthService.GetCredentials(req.Code, callback);
    }
}