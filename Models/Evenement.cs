namespace BackendEventUp.Models
{
    public class Evenement
    {
        public int id_evenement { get; set; }
        public string nom_evenement { get; set; }
        public string adresse_evenement { get; set; }
        public string date_evenement { get; set; }
        public Association listOrganisateurs { get; set; }
        public Utilisateur listUtilisateursAlerte { get; set; }
        public string description { get; set; }
        public string image { get; set; }
    }
}
