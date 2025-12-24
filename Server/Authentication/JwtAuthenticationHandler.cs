using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using GiriMovies.Server.Data;
using GiriMovies.Server.Services;

namespace GiriMovies.Server.Authentication;

public class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly GiriMoviesDbContext _context;
    private readonly ITokenService _tokenService;
    
    public JwtAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        GiriMoviesDbContext context,
        ITokenService tokenService) : base(options, logger, encoder)
    {
        _context = context;
        _tokenService = tokenService;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return AuthenticateResult.Fail("Missing Authorization Header");
        }
        
        try
        {
            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            
            if (authHeader.Scheme != "Bearer" || string.IsNullOrEmpty(authHeader.Parameter))
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
            
            var principal = _tokenService.ValidateToken(authHeader.Parameter);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
        }
    }
}
