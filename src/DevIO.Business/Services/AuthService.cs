using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevIO.Business.Configurations;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Models.Auth;
using DevIO.Business.Services.Base;
using Microsoft.AspNetCore.Identity;
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
        AppSettings appSettings) : base(notifier)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _appSettings = appSettings;
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
            Notify(_appSettings.ValidationMessages.LockedOutMessage);
            return null;
        }

        Notify(_appSettings.ValidationMessages.IncorrectUserOrPasswordMessage);
        return null;
    }

    private async Task<UserLogin> GenerateJwtAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var claimsIdentity = await GetClaimsIdentityAsync(user);
        var encodedToken = GetEncodedToken(claimsIdentity);
        var userLogin = BuildUserLogin(
            encodedToken,
            user.Id,
            user.Email,
            claimsIdentity.Claims);

        return userLogin;
    }

    private async Task<ClaimsIdentity> GetClaimsIdentityAsync(IdentityUser user)
    {
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

        return identityClaims;
    }

    private UserLogin BuildUserLogin(
        string encodedToken,
        string userId,
        string userEmail,
        IEnumerable<Claim> claims)
    {
        var userLogin = new UserLogin
        {
            AccessToken = encodedToken,
            ExpiresIn = TimeSpan.FromHours(_appSettings.AuthConfiguration.ExpirationHours).TotalSeconds,
            UserToken = new UserToken
            {
                Id = userId,
                Email = userEmail,
                Claims = claims.Select(claim => new UserClaim
                {
                    Type = claim.Type,
                    Value = claim.Value
                })
            }
        };

        return userLogin;
    }

    private string GetEncodedToken(ClaimsIdentity claimsIdentity)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.AuthConfiguration.Secret);
        var security = new SecurityTokenDescriptor
        {
            Issuer = _appSettings.AuthConfiguration.Issuer,
            Audience = _appSettings.AuthConfiguration.ValidIn,
            Subject = claimsIdentity,
            Expires = DateTime.UtcNow.AddHours(_appSettings.AuthConfiguration.ExpirationHours),
            SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(security);
        var encodedToken = tokenHandler.WriteToken(token);

        return encodedToken;
    }

    private static long ToUnixEpochDate(DateTime date) =>
        (long)Math.Round(
            (date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
}