using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectViewerEntityConfiguration : IEntityTypeConfiguration<Domain.AggregatesModel.ProjectViewer>
    {
        public void Configure(EntityTypeBuilder<Domain.AggregatesModel.ProjectViewer> builder)
        {
            builder
               .ToTable("ProjectViewers")
               .HasKey(p => p.Id);
        }
    }
}
