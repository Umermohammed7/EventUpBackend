using BackendEventUp.Models;

namespace BackendEventUp.Services
{
    public interface IRoleService
    {
        //Task<Role>
        Task<Role> CreateAssociationRoleIfNotExists(string rolePrefix, string nomAssociation);
        Task AssignerRole(int utilisateurId, string roleName);
        Task RetirerRole(int utilisateurId, string roleName);
    }

}
