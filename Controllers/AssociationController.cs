using BackendEventUp.Models;
using BackendEventUp.Models.DTO;
using BackendEventUp.Models.Requirement;
using BackendEventUp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendEventUp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssociationController : ControllerBase
    {
        private readonly Myctx _context;
        private readonly IConfiguration _configuration;
        private IRoleService _roleService;
        private readonly IAuthorizationService _authorizationService;
        private IAssociationService _IAssociationService;
        public AssociationController(IAuthorizationService authorizationService, Myctx context, IConfiguration configuration, IRoleService roleService, IAssociationService associationService)
        {
            _context = context;
            _configuration = configuration;
            _roleService = roleService;
            _authorizationService = authorizationService;
            _IAssociationService = associationService;
        }


        // Créer une association
        [Authorize]
        [HttpPost("createAssociation")]
        public async Task<IActionResult> CreationAssociation([FromForm] AssociationDTO associationDTO)
        {
            // Récupérer l'utilisateur connecté via son email dans le token
            var email = User.Identity?.Name;
            var fondateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (fondateur == null)
                return Unauthorized("Utilisateur non trouvé.");

            var association = new Association
            {
                fondateurId = fondateur.id_utilisateur,
                nom_association = associationDTO.nom_association,
                email_association = associationDTO.email_association,
                logo = associationDTO.logo,
                tag = associationDTO.tag
            };

            _context.Associations.Add(association);
            fondateur.AssociationAdhere.Add(association);
            fondateur.AssociationsCreees.Add(association);
            await _context.SaveChangesAsync();

            // Ajouter le rôle de Fondateur
            Role role = await _roleService.CreateAssociationRoleIfNotExists("Fondateur", association.nom_association);
            await _roleService.AssignerRole(fondateur.id_utilisateur, role.nom_role);

            return Ok(new { message = "Création Association réussie !" });
        }

        //Supprimer une association
        [Authorize]
        [HttpDelete("deleteAssociation/{id}")]
        public async Task<IActionResult> SupprimerAssociation(int id)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.listRole)
                .Include(u => u.AssociationsCreees)
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            var association = await _context.Associations
                .Include(a => a.Membres)
                .FirstOrDefaultAsync(a => a.id_association == id);

            if (association == null)
                return NotFound("Association introuvable.");

            if (association.fondateurId != utilisateur?.id_utilisateur)
                return Forbid("Seul le fondateur peut supprimer cette association.");

            var nomAssociation = association.nom_association;

            // Supprimer les rôles de type *_nomAssociation
            var roles = await _context.Roles
                .Where(r => r.nom_role.EndsWith($"_{nomAssociation}"))
                .ToListAsync();

            foreach (var membre in association.Membres)
            {
                foreach (var role in roles)
                {
                    await _roleService.RetirerRole(membre.id_utilisateur, role.nom_role);
                }
            }

            _context.Roles.RemoveRange(roles);
            _context.Associations.Remove(association);
            await _context.SaveChangesAsync();

            return Ok("Association supprimée avec succès.");
        }

        //Modifier une association
        [Authorize]
        [HttpPatch("editAssociation/{id}")]
        public async Task<IActionResult> EditAssociation(int id, [FromBody] AssociationDTO associationDTO)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.listRole)
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            var association = await _context.Associations.FindAsync(id);
            if (association == null)
                return NotFound();

            var nomAssociation = association.nom_association;
            var rolesUtilisateur = utilisateur.listRole.Select(r => r.nom_role).ToList();
            var estFondateur = association.fondateurId == utilisateur.id_utilisateur;
            var estAdmin = rolesUtilisateur.Contains($"Admin_{nomAssociation}");

            if (!estFondateur && !estAdmin)
                return Forbid("Accès réservé aux fondateurs ou admins.");

            // Mise à jour des champs
            if (!string.IsNullOrEmpty(associationDTO.nom_association))
                association.nom_association = associationDTO.nom_association;

            if (!string.IsNullOrEmpty(associationDTO.logo))
                association.logo = associationDTO.logo;

            if (!string.IsNullOrEmpty(associationDTO.email_association))
                association.email_association = associationDTO.email_association;

            if (!string.IsNullOrEmpty(associationDTO.tag))
                association.tag = associationDTO.tag;

            await _context.SaveChangesAsync();
            return NoContent();
        }



        private bool AssociationExists(int id) //vérifier si l'user existe
        {
            return _context.Associations.Any(e => e.id_association == id);
        }


        //Espace association
        [HttpGet("association-zone/{id_association}")]
        public async Task<IActionResult> AssociationZone(int id_association)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.listRole)
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);
            if (utilisateur == null) { return NotFound("Utilisateur introuvable"); }
            var association = await _context.Associations
               .Include(a => a.Membres)
               .FirstOrDefaultAsync(a => a.id_association == id_association);

            if (association == null)
                return NotFound("Association introuvable.");

            var rolesAutorises = new[]
            {

        $"Membre_{association.nom_association}",
        $"Admin_{association.nom_association}",
        $"Fondateur_{association.nom_association}"
    };

            bool autorise = utilisateur.listRole.Any(r => rolesAutorises.Contains(r.nom_role));
            if (!autorise)
                return Forbid("Vous n'avez pas le rôle requis pour cette action.");

            return Ok($"Bienvenue dans la zone de l'association {association.nom_association}");
        }


        //Espace admin
        [HttpGet("admin-zone/{nomAssociation}")]
        public async Task<IActionResult> AdminZone(string nomAssociation)
        {
            var result = await _authorizationService.AuthorizeAsync(User, null,
                new RoleInAssociationRequirement("Admin", nomAssociation));

            if (!result.Succeeded)
                return Forbid();

            return Ok($"Bienvenue dans la zone Admin de l'association {nomAssociation}");
        }


        //Ajouter Admin
        [HttpGet("fondator-zone/{id_association}/add_admin/{id_membre}")]
        public async Task<IActionResult> AddAdmin(int id_association, int id_membre)
        {
            var email = User.Identity?.Name;
            var utilisateurCourant = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.email_utilisateur == email);
            if (utilisateurCourant.id_utilisateur == id_membre)
            {
                return BadRequest("Vous ne pouvez pas modifier votre propre rôle d'administrateur/fondateur.");
            }

            var association = await _context.Associations
              .Include(a => a.Membres)
              .FirstOrDefaultAsync(a => a.id_association == id_association);
            var result = await _authorizationService.AuthorizeAsync(User, null,
                new RoleInAssociationRequirement("Fondateur", association.nom_association ));
            var estMembre = await _IAssociationService.EstMembreDeAssociation(id_membre, association.nom_association);

            if (!estMembre)
                return BadRequest("Ce membre ne fait pas partie de l'association.");
            

            if (!result.Succeeded)
                return Forbid();
            Role role = await _roleService.CreateAssociationRoleIfNotExists("Admin", association.nom_association);
            await _roleService.AssignerRole(id_membre, role.nom_role);
            var roleMembreAssociationAvantAdmin = "Membre_" + association.nom_association;
            await _roleService.RetirerRole(id_membre, roleMembreAssociationAvantAdmin);
            return Ok($"admin bien ajouté dans {association.nom_association}");
        }

        //Supprimer Admin
        [HttpGet("fondator-zone/{id_association}/delete_admin/{id_membre}")]
        public async Task<IActionResult> DeleteAdmin(int id_association, int id_membre)
        {
            var email = User.Identity?.Name;
            var utilisateurCourant = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.email_utilisateur == email);
            if (utilisateurCourant.id_utilisateur == id_membre)
            {
                return BadRequest("Vous ne pouvez pas modifier votre propre rôle d'administrateur/fondateur.");
            }

            var association = await _context.Associations
             .Include(a => a.Membres)
             .FirstOrDefaultAsync(a => a.id_association == id_association);
            var result = await _authorizationService.AuthorizeAsync(User, null,
                new RoleInAssociationRequirement("Fondateur", association.nom_association));
            var estMembre = await _IAssociationService.EstMembreDeAssociation(id_membre, association.nom_association);

            if (!estMembre)
                return BadRequest("Ce membre ne fait pas partie de l'association.");


            if (!result.Succeeded)
                return Forbid();
            Role role = await _roleService.CreateAssociationRoleIfNotExists("Membre", association.nom_association);
            await _roleService.AssignerRole(id_membre, role.nom_role);
            var roleMembreAssociationAvantAdmin = "Admin_" + association.nom_association;
            await _roleService.RetirerRole(id_membre, roleMembreAssociationAvantAdmin);
            return Ok($"admin bien retirer dans {association.nom_association}");
        }







        [HttpDelete("zone/{id_association}/delete_membre/{id_membre}")]
        public async Task<IActionResult> DeleteMembre(int id_association, int id_membre)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.listRole)
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (utilisateur == null)
                return Unauthorized("Utilisateur introuvable.");

            var association = await _context.Associations
                .Include(a => a.Membres)
                .FirstOrDefaultAsync(a => a.id_association == id_association);

            if (association == null)
                return NotFound("Association introuvable.");

            var rolesAutorises = new[]
            {
        $"Admin_{association.nom_association}",
        $"Fondateur_{association.nom_association}"
    };

            bool autorise = utilisateur.listRole.Any(r => rolesAutorises.Contains(r.nom_role));
            if (!autorise)
                return Forbid("Vous n'avez pas le rôle requis pour cette action.");

            var estMembre = await _IAssociationService.EstMembreDeAssociation(id_membre, association.nom_association);
            if (!estMembre)
                return BadRequest("Ce membre ne fait pas partie de l'association.");

            // Empêche de se supprimer soi-même
            if (utilisateur.id_utilisateur == id_membre)
                return BadRequest("Vous ne pouvez pas vous supprimer vous-même de l'association.");

            // Récupère le membre cible
            var membre = await _context.Utilisateurs
                .Include(u => u.listRole)
                .FirstOrDefaultAsync(u => u.id_utilisateur == id_membre);

            if (membre == null)
                return NotFound("Membre à supprimer introuvable.");

            // Empêche la suppression du fondateur
            var roleFondateur = $"Fondateur_{association.nom_association}";
            if (membre.listRole.Any(r => r.nom_role == roleFondateur))
                return BadRequest("Impossible de supprimer un membre qui est le fondateur de l'association.");

            // Supprime tous les roles liés à cette association
            var rolesARetirer = membre.listRole
    .Where(r => r.nom_role.EndsWith($"_{association.nom_association}"))
    .Select(r => r.nom_role)
    .ToList();

            foreach (var role in rolesARetirer)
            {
                await _roleService.RetirerRole(id_membre, role);
            }

            // Retirer l'association de la liste des membres adhérents
            var associationDansAdhere = membre.AssociationAdhere.FirstOrDefault(a => a.id_association == id_association);
            if (associationDansAdhere != null)
            {
                membre.AssociationAdhere.Remove(associationDansAdhere);
            }

            // Puis enregistrer les changements
            await _context.SaveChangesAsync();


            

            return Ok($"Le membre a été retiré de l'association {association.nom_association}.");
        }







        //Liste des abonnés
        [HttpGet("{id}/abonnes")]
        public async Task<IActionResult> GetAbonnesAssociation(int id)
        {
            var abonnes = await _IAssociationService.GetAbonnesAssociation(id);
            return Ok(abonnes);
        }


        //Liste des membres
        [HttpGet("{id}/membres")]
        public async Task<IActionResult> GetMembresAssociation(int id)
        {
            var membres = await _IAssociationService.GetMembresAssociation(id);
            return Ok(membres);
        }





        [HttpGet("listAssociations")] //affiche tous les associations
        public IActionResult Index()
        {

            var associations = _context.Associations
        .Select(a => new AssociationDTO
        {
            id_association = a.id_association,
            nom_association = a.nom_association,
            email_association = a.email_association,
            logo = a.logo,
            tag = a.tag
        })
        .ToList();

            if (associations.Any()) return Ok(associations);
            return NoContent();

        }


        //Liste évènements association

        [HttpGet("association/{id}/evenements")]
        public async Task<IActionResult> GetEvenementsAssociation(int id)
        {
            var association = await _context.Associations.FindAsync(id);
            if (association == null)
                return NotFound("Association introuvable.");

            var evenements = await _context.Organiser
                .Where(o => o.AssociationId == id)
                .Include(o => o.Evenement)
                .Select(o => new
                {
                    o.Evenement.id_evenement,
                    o.Evenement.nom_evenement,
                    o.Evenement.image,
                    o.Evenement.description,
                    o.DateEvenement,
                    o.AdresseEvenement
                })
                .ToListAsync();

            return Ok(evenements);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssociationParId(int id)
        {
            var association = await _context.Associations
                .Where(a => a.id_association == id)
                .Select(a => new
                {
                    a.id_association,
                    a.nom_association,
                    a.email_association,
                    a.logo,
                    // a.description,
                    Abonnes = a.Abonnes.Select(ab => new
                    {
                        ab.id_utilisateur,
                        ab.nom_utilisateur,
                        ab.prenom_utilisateur,
                        ab.email_utilisateur
                    }).ToList(),

                    Membres = a.Membres.Select(m => new
                    {
                        m.id_utilisateur,
                        m.nom_utilisateur,
                        m.prenom_utilisateur,
                        m.email_utilisateur,
                        Roles = m.listRole.Select(r => r.nom_role).ToList() // ✅ Ajout des rôles
                    }).ToList(),

                    Evenements = a.Organiser.Select(o => new
                    {
                        o.Evenement.id_evenement,
                        o.Evenement.nom_evenement,
                        o.Evenement.description,
                        o.Evenement.image,
                        o.DateEvenement,
                        o.AdresseEvenement
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (association == null)
            {
                return NotFound(new { message = "Association non trouvée" });
            }

            return Ok(association);
        }

        [Authorize]
        [HttpGet("{idAssociation}/espaceMembre")]
        public async Task<IActionResult> espaceMembre(int idAssociation)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.listRole)
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (utilisateur == null)
                return Unauthorized("Utilisateur introuvable.");

            var association = await _context.Associations
                .Include(a => a.Membres)
                .FirstOrDefaultAsync(a => a.id_association == idAssociation);

            if (association == null)
                return NotFound("Association introuvable.");
            var rolesAutorises = new[]
            {
                $"Membre_{association.nom_association}",
        $"Admin_{association.nom_association}",
        $"Fondateur_{association.nom_association}"
    };

            bool autorise = utilisateur.listRole.Any(r => rolesAutorises.Contains(r.nom_role));
            if (!autorise)
                return Forbid("Vous n'avez pas le rôle requis pour cette action.");
            return Ok("Bienvenue dans l'espace Membre de ");
        }





    }
}
