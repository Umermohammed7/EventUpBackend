namespace BackendEventUp.Models.Intermediaire
{
    public class Alerter
    {
        
        public int UtilisateurId { get; set; }
        public Utilisateur Utilisateur { get; set; }

        public int EvenementId { get; set; }
        public Evenement Evenement { get; set; }

        public DateTime DateAlerte { get; set; }
        public string StatusAlerte { get; set; } = "EnAttente";
        public string MessageAlerte { get; set; }
    }
}
