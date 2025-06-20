using Microsoft.AspNetCore.Authorization;

namespace BackendEventUp.Models.Requirement
{
    public class RoleInAssociationRequirement : IAuthorizationRequirement
    {
        public string RolePrefix { get; }
        public string AssociationName { get; }

        public RoleInAssociationRequirement(string rolePrefix, string associationName)
        {
            RolePrefix = rolePrefix;
            AssociationName = associationName;
        }
    }

}
