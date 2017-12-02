using dal.models;
using Microsoft.EntityFrameworkCore;
using System;
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

            modelBuilder.Entity<Pointage>(pt => pt.Property(p => p.Date)
                .HasColumnType("date"));
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
                var testadmin = new Utilisateur
                {
                    Centre = null,
                    Login = "testadmin",
                };

                testadmin.SetPassword("testadmin");

                this.Utilisateurs.Add(testadmin);
                this.SaveChanges();
            }
        }

        public bool ContainsCentre(int id)
        {
            return this.Centres.Any(c => c.ID == id);
        }
    }
}
