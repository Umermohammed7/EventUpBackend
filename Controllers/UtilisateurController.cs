using System.Security.Claims;
using BackendEventUp.Models;
using BackendEventUp.Models.DTO;
using BackendEventUp.Models.Intermediaire;
using BackendEventUp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendEventUp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilisateurController : ControllerBase
    {
        private readonly Myctx _context;
        private IAssociationService _IAssociationService;
        private IRoleService _IRoleService;
        private readonly EmailService _emailService;

        public UtilisateurController(Myctx context, IAssociationService IassociationService, IRoleService IroleService, EmailService emailService)
        {
            _context = context;
            _IAssociationService = IassociationService;
            _IRoleService = IroleService;
            _emailService = emailService;
        }



        [HttpGet("listUsers")] //affiche tous les users avec leur mdp
        public IActionResult Index()
        {
            return Ok(_context.Utilisateurs.ToList());
        }


        //Rejoindre association
        [Authorize]
        [HttpPost("rejoindre")]
        public async Task<IActionResult> RejoindreAssociation([FromBody] RejoindreAssociationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var email = User.Identity?.Name;

                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.AssociationAdhere)
                    .Include(u => u.listRole)
                    .FirstOrDefaultAsync(u => u.email_utilisateur == email);

                if (utilisateur == null)
                    return Unauthorized("Utilisateur introuvable.");

                var association = await _context.Associations
                    .FirstOrDefaultAsync(a => a.id_association == dto.id_association);

                if (association == null)
                    return NotFound("Association introuvable.");

                if (utilisateur.AssociationAdhere.Any(a => a.id_association == association.id_association))
                    return BadRequest("L'utilisateur a déjà rejoint cette association.");

                utilisateur.AssociationAdhere.Add(association);
                await _context.SaveChangesAsync();

                var role = await _IRoleService.CreateAssociationRoleIfNotExists("Membre", association.nom_association);
                await _IRoleService.AssignerRole(utilisateur.id_utilisateur, role.nom_role);

                utilisateur.listRole.Add(role);

                association.Membres.Add(utilisateur);
                await _context.SaveChangesAsync();

                return Ok(new { message = "L'utilisateur a bien rejoint l'association." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        [Authorize]
        [HttpPost("quitter")]
        public async Task<IActionResult> QuitterAssociation([FromBody] RejoindreAssociationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var email = User.Identity?.Name;

                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.AssociationAdhere)
                    .Include(u => u.listRole)
                    .FirstOrDefaultAsync(u => u.email_utilisateur == email);

                if (utilisateur == null)
                    return Unauthorized("Utilisateur introuvable.");

                var association = await _context.Associations
                    .FirstOrDefaultAsync(a => a.id_association == dto.id_association);

                if (association == null)
                    return NotFound("Association introuvable.");

                // Vérification si l'utilisateur est fondateur de cette association
                var roleFondateur = await _IRoleService.CreateAssociationRoleIfNotExists("Fondateur", association.nom_association);
                bool estFondateur = utilisateur.listRole.Any(r => r.nom_role == roleFondateur.nom_role);

                if (estFondateur)
                {
                    return BadRequest(new { message = "Le fondateur ne peut pas quitter son association." });
                }

                await _IAssociationService.quitterAssociation(utilisateur.id_utilisateur, association.id_association);

                var roleMembre = await _IRoleService.CreateAssociationRoleIfNotExists("Membre", association.nom_association);
                await _IRoleService.RetirerRole(utilisateur.id_utilisateur, roleMembre.nom_role);

                utilisateur.listRole.Remove(roleMembre);
                association.Membres.Remove(utilisateur);
                await _context.SaveChangesAsync();

                return Ok(new { message = "L'utilisateur a bien quitté l'association." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }




        //Abonner association
        [Authorize]
        [HttpPost("abonner")]
        public async Task<IActionResult> SuivreAssociation([FromBody] RejoindreAssociationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var email = User.Identity?.Name;

                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.AssociationAbonne)
               
                    .FirstOrDefaultAsync(u => u.email_utilisateur == email);

                if (utilisateur == null)
                    return Unauthorized("Utilisateur introuvable.");

                var association = await _context.Associations
                    .FirstOrDefaultAsync(a => a.id_association == dto.id_association);

                if (association == null)
                    return NotFound("Association introuvable.");

                if (utilisateur.AssociationAbonne.Any(a => a.id_association == association.id_association))
                    return BadRequest("L'utilisateur est déjà abonné à cette association.");

                utilisateur.AssociationAbonne.Add(association);

                association.Abonnes.Add(utilisateur);
                await _context.SaveChangesAsync();

                return Ok(new { message = "L'utilisateur est abonné l'association." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //Désabonner association
        [Authorize]
        [HttpPost("desabonner")]
        public async Task<IActionResult> DesabonnerAssociation([FromBody] RejoindreAssociationDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var email = User.Identity?.Name;

                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.AssociationAbonne)
                    
                    .FirstOrDefaultAsync(u => u.email_utilisateur == email);

                if (utilisateur == null)
                    return Unauthorized("Utilisateur introuvable.");

                var association = await _context.Associations
                    .FirstOrDefaultAsync(a => a.id_association == dto.id_association);

                if (association == null)
                    return NotFound("Association introuvable.");

                await _IAssociationService.desabonnerAssociation(utilisateur.id_utilisateur, association.id_association);

                await _context.SaveChangesAsync();

                return Ok(new { message = "L'utilisateur s'est bien désabonné." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        //[Authorize]
        //[HttpPost("evenement/{eventId}/createAlerte")]
        //public async Task<IActionResult> AlerterEvenement(int eventId, [FromBody] AlerteDTO dto)
        //{
        //    var email = User.Identity?.Name;
        //    var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.email_utilisateur == email);
        //    if (utilisateur == null) return Unauthorized();

        //    var evenement = await _context.Evenements.FindAsync(eventId);
        //    if (evenement == null) return NotFound("Événement introuvable.");

        //    var alerteExistante = await _context.Alerter.FindAsync(utilisateur.id_utilisateur, eventId);
        //    if (alerteExistante != null) return BadRequest("Vous avez déjà activé une alerte pour cet événement.");

        //    var alerte = new Alerter
        //    {
        //        UtilisateurId = utilisateur.id_utilisateur,
        //        EvenementId = eventId,
        //        DateAlerte = dto.DateAlerte,
        //        StatusAlerte = dto.StatusAlerte,
        //        MessageAlerte = dto.MessageAlerte
        //    };

        //    _context.Alerter.Add(alerte);
        //    await _context.SaveChangesAsync();

        //    // ✅ Envoi du mail de confirmation
        //    var subject = "Alerte enregistrée pour l'événement : " + evenement.nom_evenement;
        //    var body = $"Bonjour {utilisateur.nom_utilisateur},<br/><br/>" +
        //               $"Vous avez activé une alerte pour l'événement <strong>{evenement.nom_evenement}</strong>.<br/><br/>" +
        //               $"📅 Date d'alerte : {dto.DateAlerte:dd/MM/yyyy HH:mm}<br/>" +
        //               $"📩 Statut : {dto.StatusAlerte}<br/>" +
        //               $"📝 Message : {dto.MessageAlerte}<br/><br/>" +
        //               $"Merci pour votre confiance,<br/>EventUp.";

        //    await _emailService.SendEmailAsync(utilisateur.email_utilisateur, subject, body);

        //    return Ok("Alerte activée avec succès. Un email vous a été envoyé.");
        //}

        [Authorize]
        [HttpPost("evenement/{eventId}/createAlerte")]
        public async Task<IActionResult> AlerterEvenement(int eventId, [FromBody] AlerteDTO dto)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.email_utilisateur == email);
            if (utilisateur == null) return Unauthorized();

            var evenement = await _context.Evenements.FindAsync(eventId);
            if (evenement == null) return NotFound("Événement introuvable.");

            var alerteExistante = await _context.Alerter.FindAsync(utilisateur.id_utilisateur, eventId);
            if (alerteExistante != null) return BadRequest("Vous avez déjà activé une alerte pour cet événement.");

            var alerte = new Alerter
            {
                UtilisateurId = utilisateur.id_utilisateur,
                EvenementId = eventId,
                DateAlerte = dto.DateAlerte,
                StatusAlerte = "EnAttente", // ✅ forcer à true
                MessageAlerte = dto.MessageAlerte
            };

            _context.Alerter.Add(alerte);
            await _context.SaveChangesAsync();

            // ✅ Email sans StatusAlerte
            var subject = "Alerte enregistrée pour l'événement : " + evenement.nom_evenement;
            var body = $"Bonjour {utilisateur.nom_utilisateur},<br/><br/>" +
                       $"Vous avez activé une alerte pour l'événement <strong>{evenement.nom_evenement}</strong>.<br/><br/>" +
                       $"📅 Date d'alerte : {dto.DateAlerte:dd/MM/yyyy HH:mm}<br/>" +
                       $"📝 Message : {dto.MessageAlerte}<br/><br/>" +
                       $"Merci pour votre confiance,<br/>EventUp.";

            await _emailService.SendEmailAsync(utilisateur.email_utilisateur, subject, body);

            return Ok("Alerte activée avec succès. Un email vous a été envoyé.");
        }


        //[Authorize]
        //[HttpDelete("evenement/{eventId}/deleteAlerte")]
        //public async Task<IActionResult> SupprimerAlerte(int eventId)
        //{
        //    var email = User.Identity?.Name;
        //    var utilisateur = await _context.Utilisateurs
        //        .FirstOrDefaultAsync(u => u.email_utilisateur == email);
        //    if (utilisateur == null) return Unauthorized();

        //    var alerte = await _context.Alerter
        //        .FirstOrDefaultAsync(a => a.UtilisateurId == utilisateur.id_utilisateur && a.EvenementId == eventId);

        //    if (alerte == null)
        //        return NotFound("Aucune alerte active trouvée pour cet événement.");

        //    _context.Alerter.Remove(alerte);
        //    await _context.SaveChangesAsync();

        //    // ✅ Envoi d’un email de confirmation
        //    var subject = "Alerte désactivée";
        //    var body = $"Bonjour {utilisateur.nom_utilisateur},<br/><br/>" +
        //               $"Vous avez désactivé l'alerte pour l'événement ID : <strong>{eventId}</strong>.<br/><br/>" +
        //               $"Merci de nous avoir prévenus.<br/>EventUp.";

        //    await _emailService.SendEmailAsync(utilisateur.email_utilisateur, subject, body);

        //    return Ok("Alerte supprimée avec succès. Un email de confirmation a été envoyé.");
        //}


        [Authorize]
        [HttpDelete("evenement/{eventId}/deleteAlerte")]
        public async Task<IActionResult> SupprimerAlerte(int eventId)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);
            if (utilisateur == null) return Unauthorized();

            var alerte = await _context.Alerter
                .FirstOrDefaultAsync(a => a.UtilisateurId == utilisateur.id_utilisateur && a.EvenementId == eventId);

            if (alerte == null)
                return NotFound("Aucune alerte active trouvée pour cet événement.");

            // ✅ Mettre StatusAlerte à false avant suppression
            alerte.StatusAlerte = "false";
            await _context.SaveChangesAsync();

            _context.Alerter.Remove(alerte);
            await _context.SaveChangesAsync();

            // ✅ Email sans StatusAlerte
            var subject = "Alerte désactivée";
            var body = $"Bonjour {utilisateur.nom_utilisateur},<br/><br/>" +
                       $"Vous avez désactivé l'alerte pour l'événement ID : <strong>{eventId}</strong>.<br/><br/>" +
                       $"Merci de nous avoir prévenus.<br/>EventUp.";

            await _emailService.SendEmailAsync(utilisateur.email_utilisateur, subject, body);

            return Ok("Alerte supprimée avec succès. Un email de confirmation a été envoyé.");
        }


        [Authorize]
        [HttpPut("evenement/{eventId}/editAlerte")]
        public async Task<IActionResult> ModifierAlerte(int eventId, [FromBody] AlerteDTO dto)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);
            if (utilisateur == null) return Unauthorized();

            var alerte = await _context.Alerter
                .FirstOrDefaultAsync(a => a.UtilisateurId == utilisateur.id_utilisateur && a.EvenementId == eventId);

            if (alerte == null)
                return NotFound("Alerte non trouvée pour cet événement.");

            // Met à jour les propriétés de l'alerte
            alerte.DateAlerte = dto.DateAlerte;
            alerte.MessageAlerte = dto.MessageAlerte;
           // alerte.StatusAlerte = dto.StatusAlerte;

            await _context.SaveChangesAsync();

            // ✅ Envoi d’un email de confirmation de modification
            var subject = "Alerte modifiée avec succès";
            var body = $"Bonjour {utilisateur.nom_utilisateur},<br/><br/>" +
                       $"Votre alerte pour l'événement ID : <strong>{eventId}</strong> a été modifiée.<br/><br/>" +
                       $"📅 Nouvelle date d'alerte : {dto.DateAlerte:dd/MM/yyyy HH:mm}<br/>" +
                     //  $"📩 Nouveau statut : {dto.StatusAlerte}<br/>" +
                       $"📝 Nouveau message : {dto.MessageAlerte}<br/><br/>" +
                       $"Merci de rester à jour !<br/>EventUp.";

            await _emailService.SendEmailAsync(utilisateur.email_utilisateur, subject, body);

            return Ok("Alerte mise à jour avec succès. Un email de confirmation a été envoyé.");
        }


        [Authorize]
        [HttpGet("utilisateur/alertes")]
        public async Task<IActionResult> GetAlertesUtilisateur()
        {
            var email = User.Identity?.Name;

            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (utilisateur == null)
                return Unauthorized("Utilisateur non trouvé.");

            var alertes = await _context.Alerter
                .Where(a => a.UtilisateurId == utilisateur.id_utilisateur)
                .Include(a => a.Evenement)
                .ThenInclude(e => e.Organiser)
                .Select(a => new
                {
                    a.Evenement.id_evenement,
                    a.Evenement.nom_evenement,
                    a.DateAlerte,
                    a.StatusAlerte,
                    a.MessageAlerte,
                    AdresseEvenement = a.Evenement.Organiser.FirstOrDefault().AdresseEvenement,
                    DateEvenement = a.Evenement.Organiser.FirstOrDefault().DateEvenement
                })
                .ToListAsync();

            return Ok(alertes);
        }


        [HttpGet("me")]
        [Authorize] // Nécessite un token JWT valide
        public IActionResult GetMonProfil()
        {
            var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("Token invalide ou utilisateur non trouvé.");
            }

            var utilisateur = _context.Utilisateurs
                .Include(u => u.AssociationAbonne)
                .Include(u => u.AssociationAdhere)
                .FirstOrDefault(u => u.email_utilisateur == userEmail);

            var evenements = _context.Alerter
             .Where(a => a.UtilisateurId == utilisateur.id_utilisateur)
    .Select(a => new
    {
        a.Evenement.id_evenement,
        a.Evenement.nom_evenement
    })
    .ToList();

            if (utilisateur == null)
            {
                return NotFound("Utilisateur non trouvé.");
            }

            return Ok(new
            {
                utilisateur.id_utilisateur,
                utilisateur.nom_utilisateur,
                utilisateur.prenom_utilisateur,
                utilisateur.email_utilisateur,
                associationsAbonnes = utilisateur.AssociationAbonne.Select(a => new {
                    a.id_association,
                    a.nom_association,
                    a.logo,
                    a.email_association
                }),
                evenements,
                associationMembre = utilisateur.AssociationAdhere.Select(a => new {a.id_association, a.nom_association, a.logo, a.email_association})
            });
        }


        [Authorize]
        [HttpGet("getUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var email = User.Identity?.Name;
            if (email == null) return Unauthorized();

            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (utilisateur == null) return NotFound();

            return Ok(new
            {
                utilisateur.email_utilisateur,
                utilisateur.nom_utilisateur,
                utilisateur.prenom_utilisateur
            });
        }

        [Authorize]
        [HttpGet("mesAbonnements")]
        public async Task<IActionResult> GetAssociationsAbonnees()
        {
            var email = User.Identity?.Name;
            if (email == null) return Unauthorized();

            var utilisateur = await _context.Utilisateurs
                .Include(u => u.AssociationAbonne)
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (utilisateur == null) return NotFound("Utilisateur non trouvé");

            var associations = utilisateur.AssociationAbonne
                .Select(aa => new
                {
                    aa.id_association,
                    aa.nom_association,
                    aa.email_association,
                    aa.logo
                });

            return Ok(associations);
        }


        [Authorize]
        [HttpGet("etat-utilisateur/{idAssociation}")]
        public async Task<IActionResult> GetEtatUtilisateur(int idAssociation)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.AssociationAbonne)
                .Include(u => u.AssociationAdhere)
                .Include(u => u.listRole)
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (utilisateur == null)
                return Unauthorized();

            var association = await _context.Associations.FindAsync(idAssociation);
            if (association == null)
                return NotFound("Association introuvable");

            var nomAssociation = association.nom_association;

            var estAbonne = utilisateur.AssociationAbonne.Any(a => a.id_association == idAssociation);
            var estMembre = utilisateur.AssociationAdhere.Any(a => a.id_association == idAssociation);

            var estAdmin = utilisateur.listRole.Any(r => r.nom_role == $"Admin_{nomAssociation}");
            var estFondateur = utilisateur.listRole.Any(r => r.nom_role == $"Fondateur_{nomAssociation}");

            var peutAccederBackend = estAdmin || estFondateur;

            return Ok(new
            {
                estAbonne,
                estMembre,
                estAdmin,
                estFondateur,
                peutAccederBackend
            });
        }




    }
}
