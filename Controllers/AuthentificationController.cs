﻿using BackendEventUp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BackendEventUp.Models.DTO;

namespace BackendEventUp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthentificationController : ControllerBase
    {
        private readonly Myctx _context;
        private readonly IConfiguration _configuration;
        public AuthentificationController(Myctx context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }



        //[HttpPost("loginToken")]
        //public async Task<IActionResult> LoginToken([FromBody] LoginDTO dto)
        //{
        //    var utilisateur = await _context.Utilisateurs
        //        .Include(u => u.listRole)  // inclure les rôles liés
        //        .FirstOrDefaultAsync(u => u.email_utilisateur == dto.email_utilisateur && u.mdp_utilisateur == dto.mdp_utilisateur);

        //    if (utilisateur == null)
        //        return Unauthorized();

        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, utilisateur.id_utilisateur.ToString()),
        //        new Claim(ClaimTypes.Name, utilisateur.email_utilisateur),
        //    };

        //    foreach (var role in utilisateur.listRole)
        //    {
        //        claims.Add(new Claim(ClaimTypes.Role, role.nom_role));
        //    }

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration["Jwt:Issuer"],
        //        audience: _configuration["Jwt:Audience"],
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddHours(1),
        //        signingCredentials: creds
        //    );

        //    return Ok(new
        //    {
        //        token = new JwtSecurityTokenHandler().WriteToken(token),
        //        expiration = token.ValidTo
        //    });
        //}

        [HttpPost("loginToken")]
        public async Task<IActionResult> LoginToken([FromBody] LoginDTO dto)
        {
            var user = await _context.Utilisateurs
                .Include(u => u.listRole)
                .FirstOrDefaultAsync(u => u.email_utilisateur == dto.email_utilisateur);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.mdp_utilisateur, user.mdp_utilisateur))
                return Unauthorized();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.id_utilisateur.ToString()),
        new Claim(ClaimTypes.Name, user.email_utilisateur),
    };

            foreach (var role in user.listRole)
                claims.Add(new Claim(ClaimTypes.Role, role.nom_role));
            //on récupère la clef de appsettings on la convertit en bit et on l'enveloppe dans un objet utilisable par le système de sécurité aspnet
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }




        //[HttpPost("inscription")] //Création d'un client/user
        //public IActionResult Inscription([FromForm] InscriptionDTO inscriptionDto)
        //{
        //    if (ModelState.IsValid)
        //    { // Convertir le DTO en entité Utilisateur
        //        var user = new Utilisateur
        //        {
        //            nom_utilisateur = inscriptionDto.nom_utilisateur,
        //            prenom_utilisateur = inscriptionDto.prenom_utilisateur,
        //            email_utilisateur = inscriptionDto.email_utilisateur,
        //            mdp_utilisateur = inscriptionDto.mdp_utilisateur,
        //            // Les autres propriétés (comme listRole) seront ignorées ou initialisées par défaut
        //        };

        //        _context.Utilisateurs.Add(user);
        //        _context.SaveChanges();
        //        return Ok(new { message = "Inscription réussie !" });
        //    }

        //    return BadRequest(ModelState);
        //}


        [HttpPost("inscription")]
        public IActionResult Inscription([FromForm] InscriptionDTO dto)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.mdp_utilisateur);

                var user = new Utilisateur
                {
                    nom_utilisateur = dto.nom_utilisateur,
                    prenom_utilisateur = dto.prenom_utilisateur,
                    email_utilisateur = dto.email_utilisateur,
                    mdp_utilisateur = hashedPassword
                };

                _context.Utilisateurs.Add(user);
                _context.SaveChanges();
                return Ok(new { message = "Inscription réussie !" });
            }

            return BadRequest(ModelState);
        }





        [HttpGet("listUsers")] //affiche tous les users avec leur mdp
        public IActionResult Index()
        {
            return Ok(_context.Utilisateurs.ToList());
        }


        [HttpDelete("deleteUtilisateur")]
        //[Authorize(Policy = "AdminOnly")]
        public IActionResult Delete(int id) //supprimer un user avec id
        {
            var user = _context.Utilisateurs.Find(id);
            if (user != null)
            {
                _context.Entry(user).State = EntityState.Deleted;// _context.Users.Remove(user);
                _context.SaveChanges();
                return Ok("Delete complete");
            }
            _context.SaveChanges();
            return Unauthorized("Delete error id not found");
        }


        [Authorize]
        [HttpPatch("editUser")]
        public async Task<IActionResult> EditUser([FromBody] EditUserDTO userDto)
        {
            var email = User.Identity?.Name;
            if (email == null) return Unauthorized();

            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.email_utilisateur == email);

            if (utilisateur == null)
                return NotFound();

            // Mise à jour
            if (!string.IsNullOrEmpty(userDto.nom_utilisateur))
                utilisateur.nom_utilisateur = userDto.nom_utilisateur;

            if (!string.IsNullOrEmpty(userDto.prenom_utilisateur))
                utilisateur.prenom_utilisateur = userDto.prenom_utilisateur;

            if (!string.IsNullOrEmpty(userDto.email_utilisateur))
                utilisateur.email_utilisateur = userDto.email_utilisateur;

            await _context.SaveChangesAsync();
            return NoContent();
        }





        private bool UtilisateurExists(int id) //vérifier si l'user existe
        {
            return _context.Utilisateurs.Any(e => e.id_utilisateur == id);
        }


        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
        
            return Ok("Déconnexion réussie.");
        }

    }
}
