using System.ComponentModel.DataAnnotations;

namespace BackendEventUp.Models.DTO
{
    public class InscriptionDTO
    {
        [Required(ErrorMessage = "Le nom est obligatoire")]
        public string nom_utilisateur { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        public string prenom_utilisateur { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string email_utilisateur { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        public string mdp_utilisateur { get; set; }
    }
}
