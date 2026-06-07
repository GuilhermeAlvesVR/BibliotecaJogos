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
        }
    }
}