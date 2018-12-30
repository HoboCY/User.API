using MediatR;
using Microsoft.EntityFrameworkCore;
using Project.Domain.AggregatesModel;
using Project.Domain.SeedWork;
using Project.Infrastructure.EntityConfiguration;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Infrastructure
{
    public class ProjectContext : DbContext, IUnitOfWork
    {
        private readonly IMediator _mediator;
        public DbSet<Domain.AggregatesModel.Project> Projects { get; set; }
        public DbSet<ProjectContributor> ProjectContributors { get; set; }
        public DbSet<ProjectProperty> ProjectPropetries { get; set; }
        public DbSet<ProjectViewer> ProjectViewers { get; set; }
        public DbSet<ProjectVisibleRule> ProjectVisableRules { get; set; }

        public ProjectContext(DbContextOptions<ProjectContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProjectEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectContributorEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectViewerEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectPropertyEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectVisibleRuleEntityConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _mediator.DispatchDomainEventsAsync(this);

            var num = await SaveChangesAsync();
            return num > 0;
        }
    }
}
