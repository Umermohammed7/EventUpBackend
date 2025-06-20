using System.ComponentModel.DataAnnotations;

namespace BackendEventUp.Models
{
    public class Role
    {
        [Key]
        public int id_role { get; set; }
        public string nom_role { get; set; }
        public ICollection<Utilisateur> listUtilisateur {  get; set; }
        public Role() { }
        public Role(string nom) 
        {
            nom_role = nom;
        }


    }
}
