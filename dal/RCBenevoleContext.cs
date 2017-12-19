using dal.models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Linq;

namespace dal
{
    public class RCBenevoleContext : DbContext
    {
        public RCBenevoleContext(DbContextOptions options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Benevole>()
                .HasIndex(t => new { t.Nom, t.Prenom });

            modelBuilder.Entity<Frais>()
                .HasAlternateKey(f => f.Annee)
                .HasName("UQ_Frais_Annee");

            modelBuilder.Entity<Benevole>()
                .HasAlternateKey(b => new { b.Nom, b.Prenom })
                .HasName("UQ_Benevole_NomPrenom");

            modelBuilder.Entity<Pointage>()
                .HasAlternateKey(b => new { b.BenevoleID, b.Date })
                .HasName("UQ_Pointage_Benevole");

            modelBuilder.Entity<Pointage>(pt => pt.Property(p => p.Date)
                .HasColumnType("date"));

            modelBuilder.Entity<Utilisateur>()
                .HasAlternateKey(b => b.Login)
                .HasName("UQ_Utilisateur_Login");
        }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Centre> Centres { get; set; }
        public DbSet<Benevole> Benevoles { get; set; }
        public DbSet<Pointage> Pointages { get; set; }
        public DbSet<Frais> Frais { get; set; }

        public void SeedData()
        {
            this.Database.EnsureCreated();

            if(this.Utilisateurs.Count() == 0)
            {
                // ****** Centres
                var centre_paris = new Centre
                {
                    Nom = "Paris",
                    Adresse = "5 rue de Paris 75000 PARIS",
                };

                this.Centres.Add(centre_paris);

                var centre = new Centre
                {
                    Nom = "Lyon",
                    Adresse = "5 rue de Lyon 69000 Lyon",
                };

                this.Centres.Add(centre);

                // ****** Utilisateurs
                var testadmin = new Utilisateur
                {
                    Centre = null,
                    Login = "testadmin",
                };

                testadmin.SetPassword("testadmin");
                this.Utilisateurs.Add(testadmin);

                var adminparis = new Utilisateur
                {
                    Centre = centre_paris,
                    Login = "adminparis",
                };

                adminparis.SetPassword("adminparis");
                this.Utilisateurs.Add(adminparis);

                // ****** Bénévoles
                var benevole1 = new Benevole
                {
                    Prenom = "Bernard",
                    Nom = "TOTO",
                    Centre = centre,
                    AdresseLigne1 = "1 rue de david",
                    CodePostal = "69000",
                    Ville = "Lyon",
                    Telephone = "00000000",
                };

                this.Benevoles.Add(benevole1);

                this.Benevoles.Add(new Benevole
                {
                    Prenom = "Anne",
                    Nom = "TUTU",
                    Centre = centre,
                    AdresseLigne1 = "1 rue d'anne",
                    CodePostal = "13000",
                    Ville = "Marseille",
                    Telephone = "00000000",
                });

                this.Benevoles.Add(new Benevole
                {
                    Prenom = "Gérard",
                    Nom = "TITI",
                    Centre = centre_paris,
                    AdresseLigne1 = "1 rue de gérard",
                    CodePostal = "75015",
                    Ville = "Paris",
                    Telephone = "00000000",
                });

                this.Benevoles.Add(new Benevole
                {
                    Prenom = "Daniel",
                    Nom = "ROBERT",
                    Centre = centre_paris,
                    AdresseLigne1 = "1 rue de daniel",
                    CodePostal = "78000",
                    Ville = "Cergy",
                    Telephone = "00000000",
                });

                // ****** Pointages
                this.Pointages.Add(new Pointage
                {
                    Benevole = benevole1,
                    Date = new DateTime(2017, 12, 03),
                    Distance = 150,
                    NbDemiJournees = 2,
                });

                this.Pointages.Add(new Pointage
                {
                    Benevole = benevole1,
                    Date = new DateTime(2017, 12, 04),
                    Distance = 120,
                    NbDemiJournees = 2,
                });

                this.Pointages.Add(new Pointage
                {
                    Benevole = benevole1,
                    Date = new DateTime(2017, 12, 05),
                    Distance = 180,
                    NbDemiJournees = 1,
                });

                this.Pointages.Add(new Pointage
                {
                    Benevole = benevole1,
                    Date = new DateTime(2017, 12, 06),
                    Distance = 150,
                    NbDemiJournees = 1,
                });

                // ****** Frais
                this.Frais.Add(new Frais
                {
                    Annee = 2017,
                    TauxKilometrique = 0.308m,
                });

                this.Frais.Add(new Frais
                {
                    Annee = 2018,
                    TauxKilometrique = 0.308m,
                });

                this.SaveChanges();
            }
        }

        public bool ContainsCentre(int id)
        {
            return this.Centres.Any(c => c.ID == id);
        }

        public IQueryable<Benevole> ListAllowedBenevoles(Utilisateur utilisateur)
        {
            var query = this.Benevoles.Include(b => b.Centre).AsQueryable();

            if (utilisateur.CentreID != null)
                query = query.Where(b => b.CentreID == utilisateur.CentreID);

            return query.OrderBy(b => b.Nom).ThenBy(b => b.Prenom);
        }
    }
}
