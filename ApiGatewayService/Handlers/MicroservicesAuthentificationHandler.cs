using System.Security.Claims;
using System.Text.Encodings.Web;
using ApiGatewayService.Communicators;
using ApiGatewayService.Exceptions.HttpClient;
using ApiGatewayService.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ApiGatewayService.Handlers;

public class MicroservicesAuthentificationHandler : AuthenticationHandler<MicroservicesAuthenticationOptions>
{
    #region Fields

    private readonly ILogger<MicroservicesAuthentificationHandler> _logger;
    private readonly AuthCommunicator _authCommunicator;

    #endregion

    public MicroservicesAuthentificationHandler(IOptionsMonitor<MicroservicesAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock,  AuthCommunicator authCommunicator) : base(options, logger, encoder, clock)
    {
        _logger = logger.CreateLogger<MicroservicesAuthentificationHandler>();
        _authCommunicator = authCommunicator;
    }

    public MicroservicesAuthentificationHandler(IOptionsMonitor<MicroservicesAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, AuthCommunicator authCommunicator) : base(options, logger, encoder)
    {
        _logger = logger.CreateLogger<MicroservicesAuthentificationHandler>();
        _authCommunicator = authCommunicator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var token = Request.Headers["Autorization"].ToString();
            var authResult = await _authCommunicator.HandleInternalAuth(token);
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, authResult.Id.ToString()),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, authResult.Role)
            };
            var identity = new ClaimsIdentity(claims,Scheme.Name);
            
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity),Scheme.Name);
            
            return AuthenticateResult.Success(ticket);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogError(ex,ex.Message);
            return AuthenticateResult.Fail(ex.Message);
        }
        catch (BadRequestException ex)
        {
            _logger.LogError(ex,ex.Message);
            return AuthenticateResult.Fail(ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex,ex.Message);
            return AuthenticateResult.Fail(ex.Message);
        }
    }
}