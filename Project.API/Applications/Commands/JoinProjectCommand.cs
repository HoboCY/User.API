using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.Applications.Commands
{
    public class JoinProjectCommand : IRequest
    {
        public Domain.AggregatesModel.ProjectContributor Contributor { get; set; }
    }
}
