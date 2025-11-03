using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SistemaBarbearia.Models;

namespace SistemaBarbearia.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuração das tabelas do Identity para PostgreSQL
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.Id).HasColumnType("varchar(450)");
                entity.Property(u => u.UserName).HasColumnType("varchar(256)");
                entity.Property(u => u.NormalizedUserName).HasColumnType("varchar(256)");
                entity.Property(u => u.Email).HasColumnType("varchar(256)");
                entity.Property(u => u.NormalizedEmail).HasColumnType("varchar(256)");
                entity.Property(u => u.PasswordHash).HasColumnType("text");
                entity.Property(u => u.SecurityStamp).HasColumnType("text");
                entity.Property(u => u.ConcurrencyStamp).HasColumnType("text");
            });

            builder.Entity<IdentityRole>(entity =>
            {
                entity.Property(r => r.Id).HasColumnType("varchar(450)");
                entity.Property(r => r.Name).HasColumnType("varchar(256)");
                entity.Property(r => r.NormalizedName).HasColumnType("varchar(256)");
                entity.Property(r => r.ConcurrencyStamp).HasColumnType("text");
            });

            // Aqui você pode adicionar configurações para Cliente, Servico, Agendamento, se quiser
        }
    }
}
