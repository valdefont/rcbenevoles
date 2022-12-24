﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using dal;

namespace dal.Migrations
{
    [DbContext(typeof(RCBenevoleContext))]
    [Migration("20221214151203_BaremeFiscal")]
    partial class BaremeFiscal
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.11")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("dal.models.Adresse", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("AdresseLigne1")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("AdresseLigne2")
                        .HasColumnType("text");

                    b.Property<string>("AdresseLigne3")
                        .HasColumnType("text");

                    b.Property<int>("BenevoleID")
                        .HasColumnType("integer");

                    b.Property<int>("CentreID")
                        .HasColumnType("integer");

                    b.Property<string>("CodePostal")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("DateChangement")
                        .HasColumnType("timestamp without time zone");

                    b.Property<decimal>("DistanceCentre")
                        .HasColumnType("numeric");

                    b.Property<bool>("IsCurrent")
                        .HasColumnType("boolean");

                    b.Property<string>("Ville")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("CentreID");

                    b.HasIndex("BenevoleID", "DateChangement")
                        .IsUnique();

                    b.ToTable("Adresse");
                });

            modelBuilder.Entity("dal.models.BaremeFiscalLigne", b =>
                {
                    b.Property<int>("Annee")
                        .HasColumnType("integer");

                    b.Property<int>("NbChevaux")
                        .HasColumnType("integer");

                    b.Property<int>("LimiteKm")
                        .HasColumnType("integer");

                    b.Property<decimal>("Ajout")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Coef")
                        .HasColumnType("numeric");

                    b.HasKey("Annee", "NbChevaux", "LimiteKm");

                    b.ToTable("BaremeFiscalLignes");
                });

            modelBuilder.Entity("dal.models.Benevole", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("NbChevauxFiscauxVoiture")
                        .HasColumnType("integer");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Prenom")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Telephone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("Nom", "Prenom");

                    b.ToTable("Benevoles");
                });

            modelBuilder.Entity("dal.models.Centre", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Adresse")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SiegeID")
                        .HasColumnType("integer");

                    b.HasKey("ID");

                    b.HasIndex("Nom")
                        .IsUnique();

                    b.HasIndex("SiegeID");

                    b.ToTable("Centres");
                });

            modelBuilder.Entity("dal.models.Frais", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Annee")
                        .HasColumnType("integer");

                    b.Property<decimal>("TauxKilometrique")
                        .HasColumnType("numeric");

                    b.HasKey("ID");

                    b.HasIndex("Annee")
                        .IsUnique();

                    b.ToTable("Frais");
                });

            modelBuilder.Entity("dal.models.Pointage", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AdresseID")
                        .HasColumnType("integer");

                    b.Property<int>("BenevoleID")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Date")
                        .HasColumnType("date");

                    b.Property<int>("NbDemiJournees")
                        .HasColumnType("integer");

                    b.HasKey("ID");

                    b.HasIndex("AdresseID");

                    b.HasIndex("BenevoleID", "Date")
                        .IsUnique();

                    b.ToTable("Pointages");
                });

            modelBuilder.Entity("dal.models.Siege", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Adresse")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Nom")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("Nom")
                        .IsUnique();

                    b.ToTable("Sieges");
                });

            modelBuilder.Entity("dal.models.Utilisateur", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int?>("CentreID")
                        .HasColumnType("integer");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("CentreID");

                    b.HasIndex("Login")
                        .IsUnique();

                    b.ToTable("Utilisateurs");
                });

            modelBuilder.Entity("dal.models.Adresse", b =>
                {
                    b.HasOne("dal.models.Benevole", "Benevole")
                        .WithMany("Adresses")
                        .HasForeignKey("BenevoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("dal.models.Centre", "Centre")
                        .WithMany()
                        .HasForeignKey("CentreID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Benevole");

                    b.Navigation("Centre");
                });

            modelBuilder.Entity("dal.models.Centre", b =>
                {
                    b.HasOne("dal.models.Siege", "Siege")
                        .WithMany("Centres")
                        .HasForeignKey("SiegeID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Siege");
                });

            modelBuilder.Entity("dal.models.Pointage", b =>
                {
                    b.HasOne("dal.models.Adresse", "Adresse")
                        .WithMany()
                        .HasForeignKey("AdresseID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("dal.models.Benevole", "Benevole")
                        .WithMany("Pointages")
                        .HasForeignKey("BenevoleID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Adresse");

                    b.Navigation("Benevole");
                });

            modelBuilder.Entity("dal.models.Utilisateur", b =>
                {
                    b.HasOne("dal.models.Centre", "Centre")
                        .WithMany()
                        .HasForeignKey("CentreID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Centre");
                });

            modelBuilder.Entity("dal.models.Benevole", b =>
                {
                    b.Navigation("Adresses");

                    b.Navigation("Pointages");
                });

            modelBuilder.Entity("dal.models.Siege", b =>
                {
                    b.Navigation("Centres");
                });
#pragma warning restore 612, 618
        }
    }
}
