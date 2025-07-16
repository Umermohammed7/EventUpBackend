using System;
using BackendEventUp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendEventUp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly Myctx _context;

        public AdminController(Myctx context)
        {
            _context = context;
        }

        // 1️⃣ Zone réservée à l’admin
        [HttpGet("dashboard")]
        public IActionResult GetAdminDashboard()
        {
            return Ok(new { message = "Bienvenue dans la zone Admin." });
        }

        // 2️⃣ Supprimer un événement
        [HttpDelete("evenements/{id}")]
        public async Task<IActionResult> SupprimerEvenement(int id)
        {
            var evt = await _context.Evenements.FindAsync(id);
            if (evt == null)
                return NotFound();

            _context.Evenements.Remove(evt);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Événement supprimé avec succès." });
        }

        // 3️⃣ Supprimer une association
        [HttpDelete("associations/{id}")]
        public async Task<IActionResult> SupprimerAssociation(int id)
        {
            var assoc = await _context.Associations.FindAsync(id);
            if (assoc == null)
                return NotFound();

            _context.Associations.Remove(assoc);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Association supprimée avec succès." });
        }

        // 4️⃣ Supprimer un utilisateur
        [HttpDelete("utilisateurs/{id}")]
        public async Task<IActionResult> SupprimerUtilisateur(int id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.Utilisateurs.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Utilisateur supprimé avec succès." });
        }
    }
}
