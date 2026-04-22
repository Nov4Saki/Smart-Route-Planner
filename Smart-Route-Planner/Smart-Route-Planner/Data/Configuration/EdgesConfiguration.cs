using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Smart_Route_Planner.Models;

namespace Smart_Route_Planner.Data.Configuration
{
    public class EdgeConfiguration : IEntityTypeConfiguration<Edge>
    {
        public void Configure(EntityTypeBuilder<Edge> builder)
        {
            builder.HasOne(a => a.FromNode)
                .WithMany(n => n.EdgesSources)
                .HasForeignKey(e => e.FromNodeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder.HasOne(a => a.ToNode)
                .WithMany(n => n.EdgesDistinations)
                .HasForeignKey(e => e.ToNodeId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
}