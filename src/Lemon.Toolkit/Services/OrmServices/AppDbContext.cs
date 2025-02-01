using Lemon.Toolkit.Models;
using Microsoft.EntityFrameworkCore;

namespace Lemon.Toolkit.Services.OrmServices;

public class AppDbContext : DbContext
{
    public DbSet<SourceWordModel> SourceVocabulary { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql("Host=localhost;Port=5432;Database=vocabulary_db;Username=postgres;Password=1029");
}