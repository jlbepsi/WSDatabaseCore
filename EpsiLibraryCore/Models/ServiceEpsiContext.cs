using System;
using System.Configuration;
using EpsiLibraryCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EpsiLibraryCore.Models
{
    public partial class ServiceEpsiContext : DbContext
    {
        public ServiceEpsiContext()
        {
        }

        public ServiceEpsiContext(DbContextOptions<ServiceEpsiContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DatabaseDb> DatabaseDb { get; set; }
        public virtual DbSet<DatabaseGroupUser> DatabaseGroupUser { get; set; }
        public virtual DbSet<DatabaseServerName> DatabaseServerName { get; set; }
        public virtual DbSet<DatabaseServerUser> DatabaseServerUser { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConfigurationManager.ConnectionStrings["ServiceEpsiDatabase"]
                    .ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DatabaseDb>(entity =>
            {
                entity.ToTable("DatabaseDB");

                entity.Property(e => e.Commentaire).IsUnicode(false);

                entity.Property(e => e.DateCreation).HasColumnType("datetime");

                entity.Property(e => e.NomBd)
                    .IsRequired()
                    .HasColumnName("NomBD")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Server)
                    .WithMany(p => p.DatabaseDb)
                    .HasForeignKey(d => d.ServerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DatabaseDB_DatabaseServerName");
            });

            modelBuilder.Entity<DatabaseGroupUser>(entity =>
            {
                entity.HasKey(e => new { e.DbId, e.SqlLogin });

                entity.Property(e => e.SqlLogin)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.AddedByUserLogin)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserFullName)
                    .HasMaxLength(60)
                    .IsUnicode(false);

                entity.Property(e => e.UserLogin)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Db)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.DbId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DatabaseGroupUser_DatabaseDB");
            });

            modelBuilder.Entity<DatabaseServerName>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CanAddDatabase).HasDefaultValueSql("((1))");

                entity.Property(e => e.Code)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Iplocale)
                    .IsRequired()
                    .HasColumnName("IPLocale")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.NomDns)
                    .IsRequired()
                    .HasColumnName("NomDNS")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.NomDnslocal)
                    .HasColumnName("NomDNSLocal")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DatabaseServerUser>(entity =>
            {
                entity.HasKey(e => new { e.ServerId, e.SqlLogin })
                    .HasName("PK_DatabaseUserServer");

                entity.Property(e => e.SqlLogin)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserLogin)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Server)
                    .WithMany(p => p.DatabaseServerUser)
                    .HasForeignKey(d => d.ServerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DatabaseServerUser_DatabaseServerName");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
