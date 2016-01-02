

using Microsoft.Data.Entity;

namespace Pydio.Models
{
    public class PydioContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename=Pydio.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Make Blog.Url required
            modelBuilder.Entity<Server>()
                .Property(b => b.Url)
                .IsRequired();
        }
    }
}
