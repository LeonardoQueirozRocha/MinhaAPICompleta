using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevIO.Business.Configurations;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Models.Auth;
using DevIO.Business.Services.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DevIO.Business.Services;

public class AuthService : BaseService, IAuthService
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppSettings _appSettings;

    public AuthService(
        INotifier notifier,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IOptions<AppSettings> appSettings) : base(notifier)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _appSettings = appSettings.Value;
    }

    public async Task<UserLogin> CreateAsync(string email, string password)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            result.Errors
                .Select(error => error.Description)
                .ToList()
                .ForEach(error => Notify(error));

            return null;
        }

        return await GenerateJwtAsync(email);
    }

    public async Task<UserLogin> LoginAsync(string email, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(email, password, false, true);

        if (result.Succeeded)
        {
            return await GenerateJwtAsync(email);
        }

        if (result.IsLockedOut)
        {
            Notify("Usuário temporariamente bloqueado por tentativas inválidas");
            return null;
        }

        Notify("Usuário ou Senha incorretos");
        return null;
    }

    private async Task<UserLogin> GenerateJwtAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var claims = await _userManager.GetClaimsAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);

        claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
        claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
        claims.Add(new Claim(
            JwtRegisteredClaimNames.Iat,
            ToUnixEpochDate(DateTime.UtcNow).ToString(),
            ClaimValueTypes.Integer64));

        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim("role", userRole));
        }

        var identityClaims = new ClaimsIdentity();
        identityClaims.AddClaims(claims);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _appSettings.Issuer,
            Audience = _appSettings.ValidIn,
            Subject = identityClaims,
            Expires = DateTime.UtcNow.AddHours(_appSettings.ExpirationHours),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        });

        var encodedToken = tokenHandler.WriteToken(token);
        var response = new UserLogin
        {
            AccessToken = encodedToken,
            ExpiresIn = TimeSpan.FromHours(_appSettings.ExpirationHours).TotalSeconds,
            UserToken = new UserToken
            {
                Id = user.Id,
                Email = user.Email,
                Claims = claims.Select(claim => new UserClaim
                {
                    Type = claim.Type,
                    Value = claim.Value
                })
            }
        };

        return response;
    }

    private static long ToUnixEpochDate(DateTime date) =>
        (long)Math.Round(
            (date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
}