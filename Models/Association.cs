namespace BackendEventUp.Models
{
    public class Association
    {
        public int id_association { get; set; }
        public string nom_association { get; set; }
        public string fondateur { get; set; }
        public string email_association { get; set; }
        public string rib { get; set; }
        public string lienDon { get; set; }
        public Utilisateur listMembres { get; set; }
        public Utilisateur listAbonnes { get; set; }
        public Evenement listEvenements { get; set; }
        public string tag { get; set; }
        public string logo { get; set; }
    }
}
