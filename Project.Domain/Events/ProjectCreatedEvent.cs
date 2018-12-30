using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Domain.Events
{
    /// <summary>
    /// 项目被创建事件
    /// </summary>
    public class ProjectCreatedEvent : INotification
    {
        public AggregatesModel.Project Project { get; set; }
    }
}
