namespace BackendEventUp.Models.Intermediaire
{
    public class Organiser
    {
        public int AssociationId { get; set; }
        public Association Association { get; set; }

        public int EvenementId { get; set; }
        public Evenement Evenement { get; set; }

        
        public DateTime DateEvenement { get; set; }
        public string AdresseEvenement { get; set; }
    }
}
