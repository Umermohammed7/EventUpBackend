using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendEventUp.Migrations
{
    /// <inheritdoc />
    public partial class EventUpDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Evenements",
                columns: table => new
                {
                    id_evenement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom_evenement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    image = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evenements", x => x.id_evenement);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id_role = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom_role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.id_role);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    id_utilisateur = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom_utilisateur = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    prenom_utilisateur = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email_utilisateur = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mdp_utilisateur = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.id_utilisateur);
                });

            migrationBuilder.CreateTable(
                name: "Alerter",
                columns: table => new
                {
                    UtilisateurId = table.Column<int>(type: "int", nullable: false),
                    EvenementId = table.Column<int>(type: "int", nullable: false),
                    DateAlerte = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusAlerte = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageAlerte = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerter", x => new { x.UtilisateurId, x.EvenementId });
                    table.ForeignKey(
                        name: "FK_Alerter_Evenements_EvenementId",
                        column: x => x.EvenementId,
                        principalTable: "Evenements",
                        principalColumn: "id_evenement",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alerter_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "id_utilisateur",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Associations",
                columns: table => new
                {
                    id_association = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom_association = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fondateurId = table.Column<int>(type: "int", nullable: false),
                    email_association = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rib = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    lienDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    tag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    logo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Associations", x => x.id_association);
                    table.ForeignKey(
                        name: "FK_Associations_Utilisateurs_fondateurId",
                        column: x => x.fondateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "id_utilisateur",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Permettre",
                columns: table => new
                {
                    listRoleid_role = table.Column<int>(type: "int", nullable: false),
                    listUtilisateurid_utilisateur = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permettre", x => new { x.listRoleid_role, x.listUtilisateurid_utilisateur });
                    table.ForeignKey(
                        name: "FK_Permettre_Roles_listRoleid_role",
                        column: x => x.listRoleid_role,
                        principalTable: "Roles",
                        principalColumn: "id_role",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Permettre_Utilisateurs_listUtilisateurid_utilisateur",
                        column: x => x.listUtilisateurid_utilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "id_utilisateur",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Abonner",
                columns: table => new
                {
                    Abonnesid_utilisateur = table.Column<int>(type: "int", nullable: false),
                    AssociationAbonneid_association = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abonner", x => new { x.Abonnesid_utilisateur, x.AssociationAbonneid_association });
                    table.ForeignKey(
                        name: "FK_Abonner_Associations_AssociationAbonneid_association",
                        column: x => x.AssociationAbonneid_association,
                        principalTable: "Associations",
                        principalColumn: "id_association",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Abonner_Utilisateurs_Abonnesid_utilisateur",
                        column: x => x.Abonnesid_utilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "id_utilisateur",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Adherer",
                columns: table => new
                {
                    AssociationAdhereid_association = table.Column<int>(type: "int", nullable: false),
                    Membresid_utilisateur = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adherer", x => new { x.AssociationAdhereid_association, x.Membresid_utilisateur });
                    table.ForeignKey(
                        name: "FK_Adherer_Associations_AssociationAdhereid_association",
                        column: x => x.AssociationAdhereid_association,
                        principalTable: "Associations",
                        principalColumn: "id_association",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Adherer_Utilisateurs_Membresid_utilisateur",
                        column: x => x.Membresid_utilisateur,
                        principalTable: "Utilisateurs",
                        principalColumn: "id_utilisateur",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Organiser",
                columns: table => new
                {
                    AssociationId = table.Column<int>(type: "int", nullable: false),
                    EvenementId = table.Column<int>(type: "int", nullable: false),
                    DateEvenement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdresseEvenement = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organiser", x => new { x.AssociationId, x.EvenementId });
                    table.ForeignKey(
                        name: "FK_Organiser_Associations_AssociationId",
                        column: x => x.AssociationId,
                        principalTable: "Associations",
                        principalColumn: "id_association",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Organiser_Evenements_EvenementId",
                        column: x => x.EvenementId,
                        principalTable: "Evenements",
                        principalColumn: "id_evenement",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abonner_AssociationAbonneid_association",
                table: "Abonner",
                column: "AssociationAbonneid_association");

            migrationBuilder.CreateIndex(
                name: "IX_Adherer_Membresid_utilisateur",
                table: "Adherer",
                column: "Membresid_utilisateur");

            migrationBuilder.CreateIndex(
                name: "IX_Alerter_EvenementId",
                table: "Alerter",
                column: "EvenementId");

            migrationBuilder.CreateIndex(
                name: "IX_Associations_fondateurId",
                table: "Associations",
                column: "fondateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Organiser_EvenementId",
                table: "Organiser",
                column: "EvenementId");

            migrationBuilder.CreateIndex(
                name: "IX_Permettre_listUtilisateurid_utilisateur",
                table: "Permettre",
                column: "listUtilisateurid_utilisateur");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abonner");

            migrationBuilder.DropTable(
                name: "Adherer");

            migrationBuilder.DropTable(
                name: "Alerter");

            migrationBuilder.DropTable(
                name: "Organiser");

            migrationBuilder.DropTable(
                name: "Permettre");

            migrationBuilder.DropTable(
                name: "Associations");

            migrationBuilder.DropTable(
                name: "Evenements");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Utilisateurs");
        }
    }
}
