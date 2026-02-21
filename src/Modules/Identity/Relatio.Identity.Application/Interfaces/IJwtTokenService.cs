using Relatio.Identity.Application.Models;
using Relatio.Identity.Domain.Models;

namespace Relatio.Identity.Application.Interfaces;

public interface IJwtTokenService
{
    AccessTokenResult GenerateAccessToken(TokenClaims claims);
    RefreshToken GenerateRefreshToken(Guid userId);
}
