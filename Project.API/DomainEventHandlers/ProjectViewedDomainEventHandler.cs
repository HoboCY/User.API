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
    public class ProjectViewedDomainEventHandler : INotificationHandler<ProjectViewedEvent>
    {
        private ICapPublisher _capPublisher;
        public ProjectViewedDomainEventHandler(ICapPublisher capPublisher)
        {
            _capPublisher = capPublisher;
        }

        public async Task Handle(ProjectViewedEvent notification, CancellationToken cancellationToken)
        {
            var @event = new ProjectViewedIntegrationEvent
            {
                Company = notification.Company,
                Introduction = notification.Introduction,
                Viewer = notification.Viewer,
                Avatar = notification.Avatar
            };
            await _capPublisher.PublishAsync("finbook.projectapi.project_viewed", @event);
        }
    }
}
