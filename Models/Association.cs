using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BackendEventUp.Models.Intermediaire;

namespace BackendEventUp.Models
{
    public class Association
    {
        [Key]
        public int id_association { get; set; }
        public string nom_association { get; set; }
        public int fondateurId { get; set; }
        [ForeignKey("fondateurId")] //fondateurId
        public Utilisateur fondateur { get; set; }
        public string email_association { get; set; }
        public string? rib { get; set; }
        public string? lienDon { get; set; }
      
        public ICollection<Utilisateur> Membres { get; set; } = new List<Utilisateur>();
       
        public ICollection<Utilisateur> Abonnes { get; set; } = new List<Utilisateur>();

        //  public ICollection<Evenement> Evenements { get; set; } = new List<Evenement>();
        public ICollection<Organiser> Organiser { get; set; } = new List<Organiser>();
        public string tag { get; set; }
        public string logo { get; set; }

        public Association() 
        {

        }

        public Association(string Nom, string Email, Utilisateur Fondateur, string Tag, string Logo) 
        {
            nom_association = Nom;
            email_association = Email;
            fondateur = Fondateur;
            tag = Tag;
            logo = Logo;
            //Organiser.Add(new Evenement("Salon Jeunesse","Paris 5eme", new DateTime(2025,10,03), "Jeunesse", "ok"));

        }


    }
}
