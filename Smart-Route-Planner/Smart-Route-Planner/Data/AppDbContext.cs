using Microsoft.EntityFrameworkCore;
using Smart_Route_Planner.Data.Configuration;

namespace Smart_Route_Planner.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Models.Node> Nodes { get; set; }
        public DbSet<Models.Edge> Edges { get; set; }
        public DbSet<Models.Apartment> Apartments { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new ApartmentConfiguration());
        }
    }
}
