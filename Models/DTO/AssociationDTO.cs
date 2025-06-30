using System.ComponentModel.DataAnnotations;

namespace BackendEventUp.Models.DTO
{
    public class AssociationDTO
    {
        public int id_association { get; set; }
        //[Required(ErrorMessage = "Le fondateur est obligatoire")]
        //public InscriptionDTO fondateur { get; set; }
        [Required(ErrorMessage = "Le nom est obligatoire")]
        public string nom_association { get; set; }

        [Required(ErrorMessage = "Le logo est obligatoire")]
        public string logo { get; set; }
        [Required(ErrorMessage = "Le tag est obligatoire")]
        public string tag { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        public string email_association { get; set; }
        public string? description { get; set; }
        public string? rib { get; set; }
    }
}

