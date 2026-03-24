using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Relatio.Shared.Extensions;

public static class KeycloakAuthExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authority = configuration["Keycloak:Authority"]
            ?? throw new InvalidOperationException("Keycloak:Authority is not configured.");
        var audience = configuration["Keycloak:Audience"]
            ?? throw new InvalidOperationException("Keycloak:Audience is not configured.");
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var jwtIssuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var jwtAudience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.MapInboundClaims = false;
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    NameClaimType = "preferred_username",
                    RoleClaimType = "roles",
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddJwtBearer("Legacy", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme, "Legacy")
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }
}
