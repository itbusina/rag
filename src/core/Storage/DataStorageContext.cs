using core.Storage.Models;
using Microsoft.EntityFrameworkCore;

namespace core.Storage
{
    public class DataStorageContext : DbContext
    {
        private readonly string? _connectionString;
        public DbSet<DataSource> DataSources => Set<DataSource>();
        public DbSet<Assistant> Assistants => Set<Assistant>();

        public DataStorageContext(DbContextOptions<DataStorageContext> options) : base(options)
        {
        }

        // Add this:
        public DataStorageContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && _connectionString is not null)
            {
                optionsBuilder.UseSqlite($"Data Source={_connectionString}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Optional: Configure the many-to-many relationship
            modelBuilder.Entity<Assistant>()
                .HasMany(s => s.DataSources)
                .WithMany(c => c.Assistants)
                .UsingEntity(j => j.ToTable("AssistantDataSources"));
        }
    }
}