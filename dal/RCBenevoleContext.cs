using dal.models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
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
            // *** SIEGE
            modelBuilder.Entity<Siege>()
                .HasIndex(b => b.Nom)
                .IsUnique(true);

            // *** CENTRE
            modelBuilder.Entity<Centre>()
                .HasIndex(b => b.Nom)
                .IsUnique(true);

            modelBuilder.Entity<Centre>()
                .HasOne(s => s.Siege)
                .WithMany(s => s.Centres)
                .HasForeignKey(s => s.SiegeID)
                .OnDelete(DeleteBehavior.Restrict);

            // *** BENEVOLE
            modelBuilder.Entity<Benevole>()
                .HasIndex(t => new { t.Nom, t.Prenom })
                .IsUnique(false);

            modelBuilder.Entity<Benevole>()
                .HasMany(b => b.Adresses)
                .WithOne(a => a.Benevole)
                .HasForeignKey(a => a.BenevoleID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Benevole>()
                .HasMany(b => b.Vehicules)
                .WithOne(a => a.Benevole)
                .HasForeignKey(a => a.BenevoleID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Benevole>()
                .HasMany(b => b.Pointages)
                .WithOne(p => p.Benevole)
                .HasForeignKey(p => p.BenevoleID)
                .OnDelete(DeleteBehavior.Cascade);

            // *** ADRESSE
            modelBuilder.Entity<Adresse>()
                .HasOne(a => a.Centre)
                .WithMany()
                .HasForeignKey(c => c.CentreID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Adresse>()
                .HasIndex(a => new { a.BenevoleID, a.DateChangement })
                .IsUnique(true);

            // *** FRAIS
            modelBuilder.Entity<Frais>()
                .HasIndex(f => f.Annee)
                .IsUnique(true);

            // *** POINTAGE
            modelBuilder.Entity<Pointage>()
                .HasIndex(b => new { b.BenevoleID, b.Date })
                .IsUnique(true);

            modelBuilder.Entity<Pointage>(pt => pt.Property(p => p.Date)
                .HasColumnType("date"));

            // *** UTILISATEUR
            modelBuilder.Entity<Utilisateur>()
                .HasIndex(b => b.Login)
                .IsUnique(true);

            modelBuilder.Entity<Utilisateur>()
                .HasOne(u => u.Centre)
                .WithMany()
                .HasForeignKey(c => c.CentreID)
                .OnDelete(DeleteBehavior.Restrict);

			// *** BAREME FISCAUX			
			modelBuilder.Entity<BaremeFiscalLigne>()
				.HasKey(bf => new { bf.Annee, bf.NbChevaux, bf.LimiteKm });

            // ***  BAREME FISCAL DEFAUT		
            modelBuilder.Entity<BaremeFiscalDefault>()
				.HasIndex(b => b.id);

            // ***  VEHICULE		
            modelBuilder.Entity<Vehicule>()
                .HasIndex(b => b.id)
				.IsUnique(true);


        }

        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Siege> Sieges { get; set; }
        public DbSet<Centre> Centres { get; set; }
        public DbSet<Benevole> Benevoles { get; set; }
        public DbSet<Pointage> Pointages { get; set; }
        public DbSet<Frais> Frais { get; set; }
        public DbSet<Adresse> Adresse { get; set; }
		public DbSet<BaremeFiscalLigne> BaremeFiscalLignes { get; set; }
        public DbSet<BaremeFiscalDefault> BaremeFiscalDefault { get; set; }
        public DbSet<Vehicule> Vehicule { get; set; }

        public void SeedData()
        {
            this.Database.Migrate();

			// Seed utilisateurs et données 
            if(this.Utilisateurs.Count() == 0)
            {
				var seedDevData = "1";//Environment.GetEnvironmentVariable("APP_GENERATE_DEV_DATA");
				var adminPassword = "admin!2008";// Environment.GetEnvironmentVariable("APP_ADMIN_PASSWORD");

            	if (!string.IsNullOrEmpty(seedDevData) && (seedDevData == "1" || seedDevData.ToLower() == "true"))
				{
		            // ****** Sièges
		            var siege75 = new Siege
		            {
		                Nom = "AD75",
		                Adresse = "rue du siège 75000 PARIS",
		            };

		            this.Sieges.Add(siege75);

		            // ****** Centres
		            var centre_paris = new Centre
		            {
		                Nom = "Paris",
		                Adresse = "5 rue de Paris 75000 PARIS",
		                Siege = siege75,
		            };

		            this.Centres.Add(centre_paris);

		            var centre = new Centre
		            {
		                Nom = "Lyon",
		                Adresse = "5 rue de Lyon 69000 Lyon",
		                Siege = siege75,
		            };

		            this.Centres.Add(centre);

		            // ****** Utilisateurs
		            var testadmin = new Utilisateur
		            {
		                Centre = null,
		                Login = "testadmin",
		            };

					if(string.IsNullOrEmpty(adminPassword))
						adminPassword = "testadmin";

		            testadmin.SetPassword(adminPassword);
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
		                Telephone = "00000000",
		                Adresses = new List<Adresse>
		                {
		                    new Adresse
		                    {
		                        Centre = centre,
		                        AdresseLigne1 = "1 rue de david",
		                        CodePostal = "69000",
		                        Ville = "Lyon",
		                        DistanceCentre = 80,
		                    },
		                    new Adresse
		                    {
		                        DateChangement = new DateTime(2017, 2, 1),
		                        Centre = centre,
		                        AdresseLigne1 = "26 rue de david",
		                        CodePostal = "69000",
		                        Ville = "Lyon",
		                        DistanceCentre = 84,
		                    },
		                    new Adresse
		                    {
		                        DateChangement = new DateTime(2017, 3, 1),
		                        Centre = centre_paris,
		                        AdresseLigne1 = "1 rue de jules",
		                        CodePostal = "75005",
		                        Ville = "Paris",
		                        DistanceCentre = 65,
		                        IsCurrent = true,
		                    }
		                },
                        Vehicules = new List<Vehicule>
						{
							new Vehicule
							{
								NbChevaux=5,
								IsCurrent=true,
								DateChangement=new DateTime(2022,1,1),
								IsElectric=true,
							},
                            new Vehicule
                            {
                                NbChevaux=3,
                                IsCurrent=false,
                                DateChangement=new DateTime(2019,2,10),
                                IsElectric=false,
                            }

                        }
                    };

		            this.Benevoles.Add(benevole1);

		            this.Benevoles.Add(new Benevole
		            {
		                Prenom = "Anne",
		                Nom = "TUTU",
		                Telephone = "00000000",
		                Adresses = new List<Adresse>
		                {
		                    new Adresse
		                    {
		                        Centre = centre,
		                        AdresseLigne1 = "1 rue d'anne",
		                        CodePostal = "13000",
		                        Ville = "Marseille",
		                        DistanceCentre = 10,
		                        IsCurrent = true,
		                    }
		                },
                        Vehicules = new List<Vehicule>
                        {
                            new Vehicule
                            {
                                NbChevaux=7,
                                IsCurrent=true,
                                DateChangement=new DateTime(2024,5,5),
                                IsElectric=true,
                            }
                        }
                    });

		            this.Benevoles.Add(new Benevole
		            {
		                Prenom = "Gérard",
		                Nom = "TITI",
		                Telephone = "00000000",
		                Adresses = new List<Adresse>
		                {
		                    new Adresse
		                    {
		                        Centre = centre_paris,
		                        AdresseLigne1 = "1 rue de gérard",
		                        CodePostal = "75015",
		                        Ville = "Paris",
		                        DistanceCentre = 65.5m,
		                        IsCurrent = true,
		                    }
		                },
                        Vehicules = new List<Vehicule>
                        {
                            new Vehicule
                            {
                                NbChevaux=4,
                                IsCurrent=true,
                                DateChangement=new DateTime(2023,12,2),
                                IsElectric=true,
                            }
                        }
                    });

		            this.Benevoles.Add(new Benevole
		            {
		                Prenom = "Daniel",
		                Nom = "ROBERT",
		                Telephone = "00000000",
		                Adresses = new List<Adresse>
		                {
		                    new Adresse
		                    {
		                        Centre = centre_paris,
		                        AdresseLigne1 = "1 rue de daniel",
		                        CodePostal = "78000",
		                        Ville = "Cergy",
		                        DistanceCentre = 80,
		                        IsCurrent = true,
		                    }
		                },
                        Vehicules = new List<Vehicule>
                        {
                            new Vehicule
                            {
                                NbChevaux=8,
                                IsCurrent=true,
                                DateChangement=new DateTime(2016,6,6),
                                IsElectric=true,
                            }
                        }
                    });

		            // ****** Pointages
		            this.Pointages.Add(new Pointage
		            {
		                Benevole = benevole1,
		                Adresse = benevole1.Adresses.First(),
		                Date = new DateTime(2017, 1, 15),
		                NbDemiJournees = 2,
                        Vehicule = benevole1.Vehicules.First()
                    });

		            this.Pointages.Add(new Pointage
		            {
		                Benevole = benevole1,
		                Adresse = benevole1.Adresses.Skip(1).First(),
		                Date = new DateTime(2017, 2, 28),
		                NbDemiJournees = 2,
                        Vehicule = benevole1.Vehicules.First()
                    });

		            this.Pointages.Add(new Pointage
		            {
		                Benevole = benevole1,
		                Adresse = benevole1.Adresses.Skip(2).First(),
		                Date = new DateTime(2017, 03, 03),
		                NbDemiJournees = 1,
                        Vehicule = benevole1.Vehicules.First()
                    });

		            this.Pointages.Add(new Pointage
		            {
		                Benevole = benevole1,
		                Adresse = benevole1.Adresses.Skip(2).First(),
		                Date = new DateTime(2017, 03, 05),
		                NbDemiJournees = 1,
						Vehicule=benevole1.Vehicules.First()
		            });
				}
				else
				{
					// ****** Siège
		            var siege = new Siege
		            {
		                Nom = "AD68",
		                Adresse = "9 avenue d’Italie 68110 ILLZACH",
		            };

		            this.Sieges.Add(siege);

		            // ****** Utilisateurs
		            var admin = new Utilisateur
		            {
		                Centre = null,
		                Login = "admin",
		            };

					if(string.IsNullOrEmpty(adminPassword))
						adminPassword = "admin!2018";

		            admin.SetPassword(adminPassword);
		            this.Utilisateurs.Add(admin);
				}

                // ****** Frais
                this.Frais.Add(new Frais
                {
                    Annee = 2017,
                    TauxKilometrique = 0.308m,
					PourcentageVehiculeElectrique=20
                });

                this.Frais.Add(new Frais
                {
                    Annee = 2018,
                    TauxKilometrique = 0.308m,
                    PourcentageVehiculeElectrique = 20
                });

                this.SaveChanges();
            }

			if (this.BaremeFiscalDefault.Count() == 0)
			{
				this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
				{
					NbChevaux = 3,
					LimiteKm = 5000
				}
				);
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 4,
                    LimiteKm = 5000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 5,
                    LimiteKm = 5000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 6,
                    LimiteKm = 5000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 7,
                    LimiteKm = 5000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 3,
                    LimiteKm = 20000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 4,
                    LimiteKm = 20000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 5,
                    LimiteKm = 20000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 6,
                    LimiteKm = 20000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 7,
                    LimiteKm = 20000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 3,
                    LimiteKm = 1000000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 4,
                    LimiteKm = 1000000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 5,
                    LimiteKm = 1000000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 6,
                    LimiteKm = 1000000
                }
                );
                this.BaremeFiscalDefault.Add(new BaremeFiscalDefault
                {
                    NbChevaux = 7,
                    LimiteKm = 1000000
                }
                );
                this.SaveChanges();
            }

           
        }

        public bool ContainsCentre(int id)
        {
            return this.Centres.Any(c => c.ID == id);
        }

        public IQueryable<Benevole> ListAllowedBenevoles(Utilisateur utilisateur)
        {
            var query = this.Benevoles.Include(b => b.Adresses).ThenInclude(a => a.Centre).AsQueryable();

            if (utilisateur.CentreID != null)
                query = query.Where(b => b.Adresses.SingleOrDefault(a => a.IsCurrent).CentreID == utilisateur.CentreID);

            return query.OrderBy(b => b.Nom).ThenBy(b => b.Prenom);
        }
    }
}
