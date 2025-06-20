using System.ComponentModel.DataAnnotations;
using BackendEventUp.Models.Intermediaire;

namespace BackendEventUp.Models
{
    public class Evenement
    {
        [Key]
        public int id_evenement { get; set; }
        public string nom_evenement { get; set; }
        //  public ICollection<Association> Organisateurs { get; set; } = new List<Association>();
        public ICollection<Organiser> Organiser { get; set; } = new List<Organiser>();
        public ICollection<Alerter> Utilisateurs { get; set; } = new List<Alerter>();
        public string description { get; set; }
        public string image { get; set; }

        public Evenement() 
        {

        }
        public Evenement( string Description, string Image) 
        {
          //  nom_evenement = nom;
          //  adresse_evenement = adresse;
         //   date_evenement = date;
            description = Description;
            image = Image;

        }



    }
}
