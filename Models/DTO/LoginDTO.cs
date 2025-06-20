using System.ComponentModel.DataAnnotations;

namespace BackendEventUp.Models.DTO
{
    public class LoginDTO

    {
        public int id_utilisateur { get; set; }
        [Required]
        public string mdp_utilisateur { get; set; }
        [EmailAddress]
        [Required]
        public string email_utilisateur { get; set; }
    }
}
