using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectContributorEntityConfiguration : IEntityTypeConfiguration<Domain.AggregatesModel.ProjectContributor>
    {
        public void Configure(EntityTypeBuilder<Domain.AggregatesModel.ProjectContributor> builder)
        {
            builder
               .ToTable("ProjectContributors")
               .HasKey(p => p.Id);
        }
    }
}
