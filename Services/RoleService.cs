using BackendEventUp.Models;
using BackendEventUp.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class RoleService : IRoleService
{
    private readonly Myctx _context;

    public RoleService(Myctx context)
    {
        _context = context;
    }
    //Task<Role>
    // async
    public async Task<Role> CreateAssociationRoleIfNotExists(string rolePrefix, string nomAssociation)
    {
        string nomRole = $"{rolePrefix}_{nomAssociation}";
        //await
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.nom_role == nomRole);
        if (role == null)
        {
            role = new Role(nomRole);
            _context.Roles.Add(role);
            await _context.SaveChangesAsync(); //await
        }

        return role;
    }

    public async Task AssignerRole(int utilisateurId, string roleNom)
    {
        var utilisateur = await _context.Utilisateurs
            .Include(u => u.listRole)
            .FirstOrDefaultAsync(u => u.id_utilisateur == utilisateurId);

        if (utilisateur == null)
            throw new Exception("Utilisateur introuvable");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.nom_role == roleNom);

        if (role == null)
            throw new Exception("Rôle introuvable. Crée-le d’abord avec CreateAssociationRoleIfNotExists.");

        bool alreadyHasRole = utilisateur.listRole.Any(ur => ur.id_role == role.id_role);
        if (!alreadyHasRole)
        {
            utilisateur.listRole.Add(role);

            await _context.SaveChangesAsync();
        }
    }


    public async Task RetirerRole(int utilisateurId, string roleNom)
    {
        var utilisateur = await _context.Utilisateurs
            .Include(u => u.listRole)
            .FirstOrDefaultAsync(u => u.id_utilisateur == utilisateurId);

        if (utilisateur == null)
            throw new Exception("Utilisateur introuvable");
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.nom_role == roleNom);

        if (role == null)
            throw new Exception("Rôle introuvable. Crée-le d’abord avec CreateAssociationRoleIfNotExists.");

        bool alreadyHasRole = utilisateur.listRole.Any(ur => ur.id_role == role.id_role);
        if (alreadyHasRole)
        {
            utilisateur.listRole.Remove(role);

            await _context.SaveChangesAsync();
        }
    }


}


