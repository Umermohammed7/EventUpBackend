using BackendEventUp.Models;
using BackendEventUp.Models.DTO;

namespace BackendEventUp.Services
{
    public interface IAssociationService
    {
        Task rejoindreAssociation(int utilisateurId, int associationId); //Task
                                                                         // Task rejoindreAssociation(Utilisateur utilisateur, Association association);
        Task quitterAssociation(int utilisateurId, int associationId);
        Task<List<MembreDTO>> GetMembresAssociation(int associationId);
        Task<bool> EstMembreDeAssociation(int idUtilisateur, string nomAssociation);
        Task desabonnerAssociation(int utilisateurId, int associationId);
        Task<List<MembreDTO>> GetAbonnesAssociation(int associationId);

    }
}
