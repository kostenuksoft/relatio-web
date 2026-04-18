using System.Security.Claims;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Relatio.Gateway.Transforms;

public sealed class SecurityContextTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context)
    {
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
    }

    public void Apply(TransformBuilderContext context)
    {
        context.AddRequestTransform(transformContext =>
        {
            var user = transformContext.HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                return ValueTask.CompletedTask;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? user.FindFirst("sub")?.Value;

            var email = user.FindFirst(ClaimTypes.Email)?.Value
                        ?? user.FindFirst("email")?.Value;

            var roles = string.Join(",", user.FindAll(ClaimTypes.Role).Select(c => c.Value));

            if (!string.IsNullOrEmpty(userId))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);
            }

            if (!string.IsNullOrEmpty(email))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Email", email);
            }

            if (!string.IsNullOrEmpty(roles))
            {
                transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Roles", roles);
            }

            return ValueTask.CompletedTask;
        });
    }
}
