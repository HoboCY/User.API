using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using User.API.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.JsonPatch;
using User.API.Models;
using User.API.Dtos;
using DotNetCore.CAP;
using User.API.IntegrationEvents.Events;

namespace User.API.Controllers
{
    [Produces("application/json")]
    [Route("api/users")]
    public class UserController : BaseController
    {
        private UserContext _userContext;
        private ILogger<UserController> _logger;
        private ICapPublisher _capPublisher;

        public UserController(UserContext userContext, ILogger<UserController> logger, ICapPublisher capPublisher)
        {
            _userContext = userContext;
            _logger = logger;
            _capPublisher = capPublisher;
        }

        private void RaiseUserprofileChangedEvent(Models.AppUser appUser)
        {
            if (_userContext.Entry(appUser).Property(nameof(appUser.Name)).IsModified ||
                _userContext.Entry(appUser).Property(nameof(appUser.Title)).IsModified ||
                _userContext.Entry(appUser).Property(nameof(appUser.Company)).IsModified ||
                _userContext.Entry(appUser).Property(nameof(appUser.Avatar)).IsModified)
            {
                //发布
                _capPublisher.Publish("finbook.userapi.user_profile_changed", new UserProfileChangedEvent
                {
                    UserId = appUser.Id,
                    Name = appUser.Name,
                    Title = appUser.Title,
                    Company = appUser.Company,
                    Avatar = appUser.Avatar
                });
            }
        }

        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userContext.Users
                .AsNoTracking()
                .Include(u => u.Properties)
                .SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

            if (user == null)
            {
                throw new UserOperationException($"错误的用户上下文Id {UserIdentity.UserId}");
            }

            return Json(user);
        }

        [Route("")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody]JsonPatchDocument<AppUser> patch)
        {
            var user = await _userContext.Users.
                SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId);

            patch.ApplyTo(user);

            foreach (var property in user.Properties)
            {
                _userContext.Entry(property).State = EntityState.Detached;
            }

            var originProperties = await _userContext.UserProperties.AsNoTracking().
                Where(u => u.AppUserId == UserIdentity.UserId).ToListAsync(); //原有的

            var allProperties = originProperties.Union(user.Properties).Distinct(); //原有的和现在的并集去重

            var removedProperties = originProperties.Except(user.Properties);
            var newProperties = allProperties.Except(originProperties);

            foreach (var property in removedProperties)
            {
                _userContext.Remove(property);
            }

            foreach (var property in newProperties)
            {
                _userContext.Add(property);
            }

            //事务
            using (var transaction = _userContext.Database.BeginTransaction())
            {
                //发布用户变更的消息
                RaiseUserprofileChangedEvent(user);

                _userContext.Users.Update(user);
                _userContext.SaveChanges();

                transaction.Commit();
            }

            return Json(user);
        }

        [Route("check-or-create")]
        [HttpPost]
        public async Task<IActionResult> CheckOrCreate(string phone)
        {
            //TBD 做手机号码的格式验证
            var user = await _userContext.Users.SingleOrDefaultAsync(u => u.Phone == phone);

            if (user == null)
            {
                user = new AppUser { Phone = phone };
                _userContext.Users.Add(user);
                await _userContext.SaveChangesAsync();
            }
            return Ok(new
            {
                user.Id,
                user.Name,
                user.Company,
                user.Title,
                user.Avatar
            });
        }

        /// <summary>
        /// 获取用户标签选项数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("tags")]
        public async Task<IActionResult> GetUserTags()
        {
            return Ok(await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync());
        }

        /// <summary>
        /// 根据手机号码查找用户资料
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("search")]
        public async Task<IActionResult> Search(string phone)
        {
            return Ok(await _userContext.Users.Include(u => u.Properties).SingleOrDefaultAsync(u => u.Id == UserIdentity.UserId));
        }

        /// <summary>
        /// 更新用户标签数据
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("tags")]
        public async Task<IActionResult> UpdateUserTags([FromBody]List<string> tags)
        {
            var originTags = await _userContext.UserTags.Where(u => u.UserId == UserIdentity.UserId).ToListAsync();
            var newTags = tags.Except(originTags.Select(t => t.Tag));

            await _userContext.UserTags.AddRangeAsync(newTags.Select(t => new UserTag
            {
                CreatedTime = DateTime.Now,
                UserId = UserIdentity.UserId,
                Tag = t
            }));
            await _userContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Route("baseUserInfo/{userId}")]
        public async Task<IActionResult> BaseUserInfo(int userId)
        {
            ///TBD 检查用户是否好友关系
            var appUser = await _userContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (appUser == null) return NotFound();
            var baseUserInfo = new UserIdentity
            {
                Avatar = appUser.Avatar,
                Company = appUser.Company,
                Name = appUser.Name,
                Title = appUser.Title,
                UserId = appUser.Id
            };
            return Ok(baseUserInfo);
        }
    }
}