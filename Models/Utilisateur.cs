using System.ComponentModel.DataAnnotations;
using System.Data;
using BackendEventUp.Models.Intermediaire;

namespace BackendEventUp.Models
{
    public class Utilisateur
    {
        [Key]
        public int id_utilisateur { get; set; }
        public string nom_utilisateur { get; set; }
        public string prenom_utilisateur { get; set; }
        public string email_utilisateur { get; set; }
        public string mdp_utilisateur { get; set; }
        public ICollection<Association> AssociationAbonne { get; set; } = new List<Association>();
        public ICollection<Alerter> Evenements { get; set; } = new List<Alerter>();
        public ICollection<Association> AssociationAdhere { get; set; } = new List<Association>();
        public ICollection<Role> listRole { get; set; } = new List<Role>();
        public ICollection<Association> AssociationsCreees { get; set; } = new List<Association>();

        public Utilisateur() { }
        public Utilisateur(List<Role> roles, List<Association> associationsAbonne, List<Association> associationsAdhere)
        {
            listRole = roles;
            AssociationAbonne = associationsAbonne;
            AssociationAdhere = associationsAdhere;
        }



    }
}
