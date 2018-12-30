using Project.Domain.SeedWork;
using System.Threading.Tasks;

namespace Project.Domain.AggregatesModel
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<Project> GetAsync(int id);

        Task Add(Project project);

        void Update(Project project);
    }
}
