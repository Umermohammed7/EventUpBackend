using System.Data;

namespace BackendEventUp.Models
{
    public class Utilisateur
    {
        public int id_utilisateur { get; set; }
        public string nom_utilisateur { get; set; }
        public string prenom_utilisateur_ { get; set; }
        public string email_utilisateur { get; set; }
        public string mdp_utilisateur { get; set; }
        public Association listAssociationAbonne { get; set; }
        public Evenement listEvenementAlerte { get; set; }
        public Association listAssociationAdhere { get; set; }
        public Role listRole { get; set; }
    }
}
