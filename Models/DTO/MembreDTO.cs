namespace BackendEventUp.Models.DTO
{
    public class MembreDTO
    {
        public int id { get; set; }
        public string nom_membre { get; set; }
        public string email_membre { get; set; }
        public List<string> roles { get; set; }

    }
}
