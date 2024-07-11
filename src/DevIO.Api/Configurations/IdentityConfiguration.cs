using DevIO.Api.Data;
using DevIO.Api.Extensions;
using DevIO.Business.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DevIO.Api.Configurations;

public static class IdentityConfiguration
{
    public static IServiceCollection AddIdentityConfig(
        this IServiceCollection services,
        IConfiguration configuration,
        AuthConfiguration authConfiguration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddErrorDescriber<IdentityBrazilianPortugueseMessages>()
                .AddDefaultTokenProviders();

        // JWT
        var key = Encoding.ASCII.GetBytes(authConfiguration.Secret);

        services.AddAuthentication(x =>
        {
            x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = authConfiguration.ValidIn,
                ValidIssuer = authConfiguration.Issuer
            };
        });

        return services;
    }
}
