using MediatR;
using Project.Domain.AggregatesModel;
using System.Threading;
using System.Threading.Tasks;

namespace Project.API.Applications.Commands
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand>
    {
        private readonly IProjectRepository _projectRepository;

        public CreateProjectCommandHandler(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;
        }

        public async Task Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            await _projectRepository.Add(request.Project);
            await _projectRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }
}
