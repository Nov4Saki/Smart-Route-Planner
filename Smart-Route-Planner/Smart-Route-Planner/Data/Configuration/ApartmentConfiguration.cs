using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Smart_Route_Planner.Models;

namespace Smart_Route_Planner.Data.Configuration
{
    public class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
    {
        public void Configure(EntityTypeBuilder<Apartment> builder)
        {
            builder.HasOne(a => a.Node)
                .WithMany(n => n.Apartments)
                .HasForeignKey(e => e.NodeId)
                .IsRequired();
        }
    }
}