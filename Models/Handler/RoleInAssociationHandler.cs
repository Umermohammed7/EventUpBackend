using BackendEventUp.Models.Requirement;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace BackendEventUp.Models.Handler
{
    public class RoleInAssociationHandler : AuthorizationHandler<RoleInAssociationRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RoleInAssociationRequirement requirement)
        {
            var requiredRole = $"{requirement.RolePrefix}_{requirement.AssociationName}";

            if (context.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == requiredRole))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

}
