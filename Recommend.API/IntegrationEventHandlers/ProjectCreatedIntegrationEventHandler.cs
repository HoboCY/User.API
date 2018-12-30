using DotNetCore.CAP;
using Recommend.API.Data;
using Recommend.API.IntegrationEvents;
using Recommend.API.Models;
using Recommend.API.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.IntegrationEventHandlers
{
    public class ProjectCreatedIntegrationEventHandler : ICapSubscribe
    {
        private RecommendContext _context;
        private IUserService _userService;
        private IContactService _contactService;
        public ProjectCreatedIntegrationEventHandler(RecommendContext context,
            IUserService userService,
            IContactService contactService)
        {
            _context = context;
            _userService = userService;
            _contactService = contactService;
        }

        [CapSubscribe("finbook.projectapi.project_created")]
        public async Task CreateRecommendFromProject(ProjectCreatedIntegrationEvent @event)
        {
            var fromUser = await _userService.GetBaseUserInfoAsync(@event.UserId);
            var contacts = await _contactService.GetContactsByUserIdAsync(@event.UserId);
            foreach (var contact in contacts)
            {
                var recommend = new ProjectRecommend
                {
                    FromUserId = @event.UserId,
                    Company = @event.Company,
                    Introduction = @event.Introduction,
                    Tags = @event.Tags,
                    ProjectId = @event.ProjectId,
                    ProjectAvatar = @event.ProjectAvatar,
                    FinStage = @event.FinStage,
                    RecommendTime = DateTime.Now,
                    CreatedTime = @event.CreatedTime,
                    RecommendType = EnumRecommendType.Friend,
                    FromUserAvatar = fromUser.Avatar,
                    FromUserName = fromUser.Name,
                    UserId = contact.UserId
                };
                await _context.ProjectRecommends.AddAsync(recommend);
            }
            await _context.SaveChangesAsync();
        }
    }
}
