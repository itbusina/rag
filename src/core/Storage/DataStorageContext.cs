using core.Storage.Models;
using Microsoft.EntityFrameworkCore;

namespace core.Storage
{
    public class DataStorageContext(DbContextOptions<DataStorageContext> options) : DbContext(options)
    {
        public DbSet<DataSource> DataSources => Set<DataSource>();
        public DbSet<Assistant> Assistants => Set<Assistant>();

        // protected override void OnConfiguring(DbContextOptionsBuilder options)
        //     => options.UseSqlite("Data Source=data/data.db");

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