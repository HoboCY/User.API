using MediatR;
using Project.Domain.AggregatesModel;
using System.Threading;
using System.Threading.Tasks;

namespace Project.API.Applications.Commands
{
    public class JoinProjectCommandHandler : IRequestHandler<JoinProjectCommand>
    {
        private IProjectRepository _projectRepository;
        public JoinProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task Handle(JoinProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _projectRepository.GetAsync(request.Contributor.ProjectId);
            if (project == null)
            {
                throw new Domain.Exceptions.ProjectDomainException($"project not found:{ request.Contributor.ProjectId }");
            }
            if (project.UserId == request.Contributor.UserId)
            {
                throw new Domain.Exceptions.ProjectDomainException($"You cannot join your own project:{request.Contributor.UserId}");
            }
            project.AddContributor(request.Contributor);
            await _projectRepository.UnitOfWork.SaveEntitiesAsync();
        }
    }
}
