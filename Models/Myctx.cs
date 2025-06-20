using BackendEventUp.Models.Intermediaire;
using Microsoft.EntityFrameworkCore;

namespace BackendEventUp.Models
{
    public class Myctx : DbContext
    {
        public Myctx(DbContextOptions<Myctx> options) : base(options) { }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Association> Associations { get; set; }
        public DbSet<Evenement> Evenements { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Organiser> Organiser { get; set; }
        public DbSet<Alerter> Alerter { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relation Utilisateur-Role (many-to-many)
            modelBuilder.Entity<Utilisateur>()
                .HasMany(u => u.listRole)
                .WithMany(r => r.listUtilisateur)
                .UsingEntity(j => j.ToTable("Permettre"));

            // Relation Association membres (many-to-many)
            modelBuilder.Entity<Association>()
                .HasMany(a => a.Membres)
                .WithMany(u => u.AssociationAdhere)
                .UsingEntity(j => j.ToTable("Adherer"));

            // Relation Association abonnés (many-to-many)
            modelBuilder.Entity<Association>()
                .HasMany(a => a.Abonnes)
                .WithMany(u => u.AssociationAbonne)
                .UsingEntity(j => j.ToTable("Abonner"));

            modelBuilder.Entity<Organiser>()
                .HasKey(ae => new { ae.AssociationId, ae.EvenementId });

            modelBuilder.Entity<Organiser>()
                .HasOne(ae => ae.Association)
                .WithMany(a => a.Organiser)
                .HasForeignKey(ae => ae.AssociationId)
                .OnDelete(DeleteBehavior.Cascade); // cascade suppression des liaisons

            modelBuilder.Entity<Organiser>()
                .HasOne(ae => ae.Evenement)
                .WithMany(e => e.Organiser)
                .HasForeignKey(ae => ae.EvenementId)
                .OnDelete(DeleteBehavior.Cascade); // cascade suppression des événements si besoin


            modelBuilder.Entity<Alerter>()
                .HasKey(a => new { a.UtilisateurId, a.EvenementId });

            modelBuilder.Entity<Alerter>()
                .HasOne(a => a.Utilisateur)
                .WithMany(u => u.Evenements)
                .HasForeignKey(a => a.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Alerter>()
                .HasOne(a => a.Evenement)
                .WithMany(e => e.Utilisateurs)
                .HasForeignKey(a => a.EvenementId)
                .OnDelete(DeleteBehavior.Cascade);
    

            modelBuilder.Entity<Association>()
                .HasOne(a => a.fondateur)
                .WithMany(u => u.AssociationsCreees)
                .HasForeignKey(a => a.fondateurId)
                .OnDelete(DeleteBehavior.Restrict); // important pour éviter la suppression en cascade


        }



    }
}
