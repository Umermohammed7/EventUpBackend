namespace BackendEventUp.Models.DTO
{
    public class AlerteDTO
    {
        //public int UtilisateurId { get; set; }
        //public int EvenementId { get; set; }
        public DateTime DateAlerte { get; set; }
        public string? StatusAlerte { get; set; }
        public string MessageAlerte { get; set; }
    }
}
