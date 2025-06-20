using System.ComponentModel.DataAnnotations;

namespace BackendEventUp.Models.DTO
{
    public class EditUserDTO
    {
        public int id_utilisateur { get; set; }
        public string? nom_utilisateur { get; set; }
        public string? prenom_utilisateur { get; set; }
        [EmailAddress]
        public string? email_utilisateur { get; set; }

    }
}
