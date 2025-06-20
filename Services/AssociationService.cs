using BackendEventUp.Models;
using BackendEventUp.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendEventUp.Services
{
    public class AssociationService : IAssociationService
    {
        private readonly Myctx _context;
        private IRoleService _IRoleService;
        


        public AssociationService(Myctx context, IRoleService iRoleService)
        {
            _context = context;
            _IRoleService = iRoleService;
            
        }


        //public async Task rejoindreAssociation(Utilisateur utilisateur, Association association)
        //{
        //   // Vérifie si l'association existe aussi
        //    var associationInDb = await _context.Associations
        //        .FirstOrDefaultAsync(a => a.id_association == association.id_association);
        //    System.Diagnostics.Debug.WriteLine(" Voici l'asso après tout " + associationInDb.nom_association);
        //    // Vérifie si l'utilisateur existe bien dans la base
        //    var userInDb = await _context.Utilisateurs
        //        .Include(u => u.AssociationAdhere)
        //        .FirstOrDefaultAsync(u => u.id_utilisateur == utilisateur.id_utilisateur);
        //    System.Diagnostics.Debug.WriteLine(" Voici le nom direct après le context " + userInDb.nom_utilisateur);

        //    System.Diagnostics.Debug.WriteLine(" Voici le nom   après tout " + userInDb.nom_utilisateur);

        //    System.Diagnostics.Debug.WriteLine(" Voici le truc 11111111111111111111 " + associationInDb.id_association);
        //    if (associationInDb == null)
        //        throw new Exception("association introuvable.");
        //    if (userInDb == null)
        //        throw new Exception("Utilisateur introuvable.");

        //    if (userInDb.AssociationAdhere.Any(a => a.id_association == associationInDb.id_association))
        //        throw new Exception("L'utilisateur a déjà rejoint cette association.");

        //    userInDb.AssociationAdhere.Add(associationInDb);
        //    await _context.SaveChangesAsync();
        //}


        public async Task rejoindreAssociation(int utilisateurId, int associationId)
        {
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.AssociationAdhere)
                .FirstOrDefaultAsync(u => u.id_utilisateur == utilisateurId);

            if (utilisateur == null)
                throw new Exception("Utilisateur introuvable.");
            System.Diagnostics.Debug.WriteLine(utilisateur.nom_utilisateur);
            var association = await _context.Associations
                .FirstOrDefaultAsync(a => a.id_association == associationId);

            if (association == null)
                throw new Exception("Association introuvable.");

            if (utilisateur.AssociationAdhere.Any(a => a.id_association == associationId))
                throw new Exception("L'utilisateur a déjà rejoint cette association.");

            utilisateur.AssociationAdhere.Add(association);
            await _context.SaveChangesAsync();
        }

        public async Task quitterAssociation(int utilisateurId, int associationId) 
        {
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.AssociationAdhere)
                .FirstOrDefaultAsync(u => u.id_utilisateur == utilisateurId);

            if (utilisateur == null)
                throw new Exception("Utilisateur introuvable.");
            System.Diagnostics.Debug.WriteLine(utilisateur.nom_utilisateur);
            var association = await _context.Associations
                .FirstOrDefaultAsync(a => a.id_association == associationId);

            if (association == null)
                throw new Exception("Association introuvable.");

            if (!utilisateur.AssociationAdhere.Any(a => a.id_association == associationId))
                throw new Exception("L'utilisateur n'est pas membre de cette association.");

            utilisateur.AssociationAdhere.Remove(association);
            await _context.SaveChangesAsync();
        }


        public async Task desabonnerAssociation(int utilisateurId, int associationId)
        {
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.AssociationAbonne)
                .FirstOrDefaultAsync(u => u.id_utilisateur == utilisateurId);

            if (utilisateur == null)
                throw new Exception("Utilisateur introuvable.");
            System.Diagnostics.Debug.WriteLine(utilisateur.nom_utilisateur);
            var association = await _context.Associations
                .FirstOrDefaultAsync(a => a.id_association == associationId);

            if (association == null)
                throw new Exception("Association introuvable.");

            if (!utilisateur.AssociationAbonne.Any(a => a.id_association == associationId))
                throw new Exception("L'utilisateur n'est pas abonné à cette association.");

            utilisateur.AssociationAbonne.Remove(association);
            association.Abonnes.Remove(utilisateur);
            await _context.SaveChangesAsync();
        }




        public async Task<List<MembreDTO>> GetMembresAssociation(int associationId)
        {
            var association = await _context.Associations
                .Where(a => a.id_association == associationId)
                .Include(a => a.Membres)
                    .ThenInclude(u => u.listRole)
                .FirstOrDefaultAsync();

            if (association == null)
                return new List<MembreDTO>();

            var nomAssociation = association.nom_association.ToLower();//Replace(" ", "").

            var membres = association.Membres
                .Select(m => new MembreDTO
                {
                    id = m.id_utilisateur,
                    nom_membre = m.nom_utilisateur,
                    email_membre = m.email_utilisateur,
                    roles = m.listRole
                        .Where(r => r.nom_role.ToLower().EndsWith($"_{nomAssociation}"))
                        .Select(r => r.nom_role)
                        .ToList()
                })
                .ToList();

            return membres;
        }

        public async Task<bool> EstMembreDeAssociation(int idUtilisateur, string nomAssociation)
        {
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.AssociationAdhere)
                .FirstOrDefaultAsync(u => u.id_utilisateur == idUtilisateur);

            if (utilisateur == null)
                return false;

            var association = await _context.Associations
                .FirstOrDefaultAsync(a => a.nom_association == nomAssociation);

            if (association == null)
                return false;

            return utilisateur.AssociationAdhere.Any(a => a.id_association == association.id_association);
        }


        public async Task<List<MembreDTO>> GetAbonnesAssociation(int associationId)
        {
            var association = await _context.Associations
                .Where(a => a.id_association == associationId)
                .Include(a => a.Abonnes)
                    .ThenInclude(u => u.listRole)
                .FirstOrDefaultAsync();

            if (association == null)
                return new List<MembreDTO>();

            var nomAssociation = association.nom_association.ToLower();//Replace(" ", "").

            var abonnes = association.Abonnes
                .Select(m => new MembreDTO
                {
                    id = m.id_utilisateur,
                    nom_membre = m.nom_utilisateur,
                    email_membre = m.email_utilisateur,
                    roles = m.listRole
                        .Where(r => r.nom_role.ToLower().EndsWith($"_{nomAssociation}"))
                        .Select(r => r.nom_role)
                        .ToList()
                })
                .ToList();

            return abonnes;
        }



    }
}
