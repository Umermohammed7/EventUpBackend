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
    public class EvenementController : ControllerBase
    {
        private readonly Myctx _context;
        private readonly EmailService _emailService;
        public EvenementController(Myctx context, EmailService emailService) 
        {
            _context = context;
            _emailService = emailService;
        }



        [Authorize]
        [HttpPost("association/{id}/evenement")]
        public async Task<IActionResult> CreateEvenement(int id, [FromBody] CreateEvenementDTO dto)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.listRole)
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (utilisateur == null)
                return Unauthorized("Utilisateur introuvable.");

            var association = await _context.Associations
                .Include(a => a.Abonnes)
                .FirstOrDefaultAsync(a => a.id_association == id);

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
                return Forbid("Vous n'avez pas le rôle requis pour créer un événement dans cette association.");

            var evenement = new Evenement
            {
                nom_evenement = dto.Titre,
                image = dto.image,
                description = dto.description
            };

            var link = new Organiser
            {
                Association = association,
                Evenement = evenement,
                DateEvenement = dto.DateEvenement,
                AdresseEvenement = dto.AdresseEvenement
            };

            _context.Add(link);
            await _context.SaveChangesAsync();

            // Envoi d'emails aux abonnés
            foreach (var abonne in association.Abonnes)
            {
                var subject = $"Nouveau événement dans {association.nom_association} : {dto.Titre}";
                var body = $"Bonjour {abonne.nom_utilisateur},<br/><br/>" +
                           $"Un nouvel événement a été créé par l'association <strong>{association.nom_association}</strong> :<br/>" +
                           $"<strong>{dto.Titre}</strong><br/>" +
                           $"<em>{dto.description}</em><br/><br/>" +
                           $"📅 Le {dto.DateEvenement:dd/MM/yyyy HH:mm}<br/>" +
                           $"📍 Adresse : {dto.AdresseEvenement}<br/><br/>" +
                           $"Merci de votre attention !<br/>EventUp";

                await _emailService.SendEmailAsync(abonne.email_utilisateur, subject, body);
            }

            return Ok("Événement ajouté et emails envoyés.");
        }




        [HttpGet("association/{id}/evenements")]
        public async Task<IActionResult> GetEvenementsAssociation(int id)
        {
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



        [Authorize]
        [HttpPut("association/{id}/evenement/updateEvenement/{eventId}")]
        public async Task<IActionResult> UpdateEvenement(int id, int eventId, [FromBody] CreateEvenementDTO dto)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.listRole)
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (utilisateur == null) return Unauthorized();

            var association = await _context.Associations
                .Include(a => a.Abonnes)
                .FirstOrDefaultAsync(a => a.id_association == id);
            if (association == null) return NotFound("Association introuvable.");

            var rolesAutorises = new[]
            {
        $"Membre_{association.nom_association}",
        $"Admin_{association.nom_association}",
        $"Fondateur_{association.nom_association}"
    };

            if (!utilisateur.listRole.Any(r => rolesAutorises.Contains(r.nom_role)))
                return Forbid("Accès interdit.");

            var lien = await _context.Organiser
                .Include(o => o.Evenement)
                .FirstOrDefaultAsync(o => o.AssociationId == id && o.EvenementId == eventId);
            if (lien == null) return NotFound("Événement non trouvé.");

            lien.Evenement.nom_evenement = dto.Titre;
            lien.Evenement.image = dto.image;
            lien.Evenement.description = dto.description;
            lien.DateEvenement = dto.DateEvenement;
            lien.AdresseEvenement = dto.AdresseEvenement;

            await _context.SaveChangesAsync();

            // 🔔 Notifier les abonnés
            foreach (var abonne in association.Abonnes)
            {
                var subject = $"Événement modifié : {dto.Titre}";
                var body = $"Bonjour {abonne.nom_utilisateur},<br/><br/>" +
                           $"L'événement <strong>{dto.Titre}</strong> de l'association <strong>{association.nom_association}</strong> a été mis à jour.<br/><br/>" +
                           $"🗓️ Nouvelle date : {dto.DateEvenement:dd/MM/yyyy HH:mm}<br/>" +
                           $"📍 Lieu : {dto.AdresseEvenement}<br/>" +
                           $"📌 Description : {dto.description}<br/><br/>" +
                           $"Merci de rester informé !<br/>EventUp";

                await _emailService.SendEmailAsync(abonne.email_utilisateur, subject, body);
            }

            return Ok("Événement mis à jour.");
        }



        [Authorize]
        [HttpDelete("association/{id}/evenement/deleteEvenement/{eventId}")]
        public async Task<IActionResult> DeleteEvenement(int id, int eventId)
        {
            var email = User.Identity?.Name;
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.listRole)
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (utilisateur == null) return Unauthorized();

            var association = await _context.Associations
                .Include(a => a.Abonnes)
                .FirstOrDefaultAsync(a => a.id_association == id);
            if (association == null) return NotFound("Association introuvable.");

            var rolesAutorises = new[]
            {
        $"Membre_{association.nom_association}",
        $"Admin_{association.nom_association}",
        $"Fondateur_{association.nom_association}"
    };

            if (!utilisateur.listRole.Any(r => rolesAutorises.Contains(r.nom_role)))
                return Forbid("Accès interdit.");

            var organiser = await _context.Organiser
                .Include(o => o.Evenement)
                .FirstOrDefaultAsync(o => o.AssociationId == id && o.EvenementId == eventId);
            if (organiser == null) return NotFound("Événement introuvable.");

            var titre = organiser.Evenement.nom_evenement;

            _context.Evenements.Remove(organiser.Evenement);
            await _context.SaveChangesAsync();

            // 🔔 Notifier les abonnés
            foreach (var abonne in association.Abonnes)
            {
                var subject = $"Événement annulé : {titre}";
                var body = $"Bonjour {abonne.nom_utilisateur},<br/><br/>" +
                           $"L'événement <strong>{titre}</strong> de l'association <strong>{association.nom_association}</strong> a été annulé.<br/><br/>" +
                           $"Merci de votre compréhension.<br/>EventUp";

                await _emailService.SendEmailAsync(abonne.email_utilisateur, subject, body);
            }

            return Ok("Événement supprimé.");
        }


        [HttpGet("evenements")]
        public async Task<IActionResult> GetTousLesEvenements()
        {
            var evenements = await _context.Organiser
                .Include(o => o.Evenement)
                .Include(o => o.Association)
                .Select(o => new
                {
                    o.Evenement.id_evenement,
                    o.Evenement.nom_evenement,
                    o.Evenement.image,
                    o.Evenement.description,
                    o.DateEvenement,
                    o.AdresseEvenement,
                    Association = new
                    {
                        o.Association.id_association,
                        o.Association.nom_association,
                        o.Association.logo
                    }
                })
                .ToListAsync();

            return Ok(evenements);
        }


        [HttpGet("evenement/{id}")]
        public async Task<IActionResult> GetEvenementById(int id)
        {
            var evenement = await _context.Organiser
                .Include(o => o.Evenement)
                .Include(o => o.Association)
                .Where(o => o.Evenement.id_evenement == id)
                .Select(o => new
                {
                    o.Evenement.id_evenement,
                    o.Evenement.nom_evenement,
                    o.Evenement.image,
                    o.Evenement.description,
                    o.DateEvenement,
                    o.AdresseEvenement,
                    Association = new
                    {
                        o.Association.id_association,
                        o.Association.nom_association,
                        o.Association.logo
                    }
                })
                .FirstOrDefaultAsync();

            if (evenement == null)
            {
                return NotFound(new { message = "Évènement introuvable." });
            }

            return Ok(evenement);
        }






    }
}
