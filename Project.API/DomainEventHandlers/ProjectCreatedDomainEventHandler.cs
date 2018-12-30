using DotNetCore.CAP;
using MediatR;
using Project.API.IntegrationEvents;
using Project.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Project.API.DomainEventHandlers
{
    public class ProjectCreatedDomainEventHandler : INotificationHandler<ProjectCreatedEvent>
    {
        private readonly ICapPublisher _capPublisher;
        public ProjectCreatedDomainEventHandler(ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
        }

        public async Task Handle(ProjectCreatedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectCreatedIntegrationEvent
            {
                CreatedTime = DateTime.Now,
                UserId = notification.Project.UserId,
                Company = notification.Project.Company,
                FinStage = notification.Project.FinStage,
                Introduction = notification.Project.Introduction,
                ProjectAvatar = notification.Project.Avatar,
                Tags = notification.Project.Tags,
                ProjectId = notification.Project.Id
            };
            await _capPublisher.PublishAsync("finbook.projectapi.project_created", @event);
        }
    }
}
