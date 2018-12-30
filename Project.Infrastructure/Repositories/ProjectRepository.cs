using Microsoft.EntityFrameworkCore;
using Project.Domain.AggregatesModel;
using Project.Domain.SeedWork;
using System.Threading.Tasks;
using ProjectEntity = Project.Domain.AggregatesModel.Project;

namespace Project.Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ProjectContext _context;
        public IUnitOfWork UnitOfWork => _context;

        public ProjectRepository(ProjectContext context)
        {
            _context = context;
        }

        public async Task Add(ProjectEntity project)
        {
            if (project.IsTransient())  //判断主键是否是初始值
            {
                await _context.Projects.AddAsync(project);
            }
        }

        public async Task<ProjectEntity> GetAsync(int id)
        {
            var project = await _context.Projects
                 .Include(p => p.Properties)
                 .Include(p => p.Viewers)
                 .Include(p => p.Contributors)
                 .Include(p => p.VisibleRule)
                 .SingleOrDefaultAsync();
            return project;
        }

        public void Update(ProjectEntity project)
        {
            _context.Update(project);
        }
    }
}
