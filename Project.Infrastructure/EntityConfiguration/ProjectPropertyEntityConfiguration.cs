using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectPropertyEntityConfiguration : IEntityTypeConfiguration<Domain.AggregatesModel.ProjectProperty>
    {
        public void Configure(EntityTypeBuilder<Domain.AggregatesModel.ProjectProperty> builder)
        {
            builder.ToTable("ProjectProperties").Property(p => p.Key).HasMaxLength(100);
            builder.ToTable("ProjectProperties").Property(p => p.Value).HasMaxLength(100);
            builder.HasKey(p => new { p.ProjectId, p.Key, p.Value });
        }
    }
}
