using System.ComponentModel.DataAnnotations;

namespace BackendEventUp.Models.DTO
{
    public class InscriptionDTO
    {
        //[Required(ErrorMessage = "Le nom est obligatoire")]
        //public string nom_utilisateur { get; set; }

        //[Required(ErrorMessage = "Le prénom est obligatoire")]
        //public string prenom_utilisateur { get; set; }

        //[Required(ErrorMessage = "L'email est obligatoire")]
        //[EmailAddress(ErrorMessage = "Format d'email invalide")]
        //public string email_utilisateur { get; set; }

        //[Required(ErrorMessage = "Le mot de passe est obligatoire")]
        //[MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        //public string mdp_utilisateur { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        public string nom_utilisateur { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        public string prenom_utilisateur { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string email_utilisateur { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [MinLength(15, ErrorMessage = "Le mot de passe doit contenir au moins 15 caractères")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).+$", ErrorMessage = "Le mot de passe doit contenir au moins une minuscule, une majuscule et un caractère spécial")]
        public string mdp_utilisateur { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire")]
        [Compare("mdp_utilisateur", ErrorMessage = "Les mots de passe ne correspondent pas")]
        public string conf_mdp_utilisateur { get; set; }
    }
}
