using System;
using System.Collections.Generic;
using System.Text;

using BibliotecaJogos.Models;
using Microsoft.EntityFrameworkCore;



namespace BibliotecaJogos.Data
{
    public class BibliotecaContext : DbContext
    {
        public DbSet<Jogo> Jogos { get; set; }
        public DbSet<Avaliacao> Avaliacoes { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=biblioteca.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relacionamento: Um Jogo tem muitas Avaliações
            modelBuilder.Entity<Avaliacao>()
                .HasOne(a => a.Jogo)
                .WithMany(j => j.Avaliacoes)
                .HasForeignKey(a => a.JogoId);

            // Relacionamento: Um Jogo tem varias Tags e uma Tag pertence a varios Jogos
            modelBuilder.Entity<Jogo>()
                .HasMany(j => j.Tags)
                .WithMany(t => t.Jogos)
                .UsingEntity(j => j.ToTable("JogoTag"));
        }
    }
}
