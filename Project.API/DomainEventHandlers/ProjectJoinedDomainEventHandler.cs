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
    public class ProjectJoinedDomainEventHandler : INotificationHandler<ProjectJoinedEvent>
    {
        private ICapPublisher _capPublisher;
        public ProjectJoinedDomainEventHandler(ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
        }

        public async Task Handle(ProjectJoinedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectJoinedIntegrationEvent
            {
                Company = notification.Company,
                Introduction = notification.Introduction,
                Contributor = notification.Contributor,
                Avatar = notification.Avatar
            };
            await _capPublisher.PublishAsync("finbook.projectapi.project_joined", @event);
        }
    }
}
