using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectVisibleRuleEntityConfiguration : IEntityTypeConfiguration<Domain.AggregatesModel.ProjectVisibleRule>
    {
        public void Configure(EntityTypeBuilder<Domain.AggregatesModel.ProjectVisibleRule> builder)
        {
            builder
               .ToTable("ProjectVisibleRules")
               .HasKey(p => p.Id);
        }
    }
}
