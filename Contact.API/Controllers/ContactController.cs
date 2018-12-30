using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contact.API.Data;
using Contact.API.Models;
using Contact.API.Services;
using Contact.API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Contact.API.Controllers
{
    [Route("api/contacts")]
    public class ContactController : BaseController
    {
        private IContactRepository _contactRepository;
        private IContactApplyRequestRepository _contactApplyRequestRepository;
        private IUserService _userService;

        public ContactController(IContactRepository contactRepository,
            IContactApplyRequestRepository contactApplyRequestRepository,
            IUserService userService)
        {
            _contactRepository = contactRepository;
            _contactApplyRequestRepository = contactApplyRequestRepository;
            _userService = userService;
        }

        /// <summary>
        /// 获取通讯录列表
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            return Ok(await _contactRepository.GetContactAsync(UserIdentity.UserId, cancellationToken));
        }

        /// <summary>
        /// 获取通讯录列表
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> Get(int userId, CancellationToken cancellationToken)
        {
            return Ok(await _contactRepository.GetContactAsync(userId, cancellationToken));
        }

        /// <summary>
        /// 更新好友标签
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("tag")]
        public async Task<IActionResult> TagContact([FromBody]TagContactInputViewModel viewModel, CancellationToken cancellationToken)
        {
            var result = await _contactRepository.TagContactAsync(UserIdentity.UserId, viewModel.ContactId, viewModel.Tags, cancellationToken);
            if (result)
            {
                return Ok();
            }

            //LOG TBD
            return BadRequest();
        }

        /// <summary>
        /// 获取好友申请列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("apply-requests")]
        public async Task<IActionResult> GetApplyRequests(CancellationToken cancellationToken)
        {
            var requests = await _contactApplyRequestRepository.GetRequestListAsync(UserIdentity.UserId, cancellationToken);
            return Ok(requests);
        }

        /// <summary>
        /// 添加好友请求
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("apply-requests/{userId}")]
        public async Task<IActionResult> AddApplyRequest(int userId, CancellationToken cancellationToken)
        {
            var result = await _contactApplyRequestRepository.AddRequestAsync(new ContactApplyRequest
            {
                UserId = userId,
                ApplierId = UserIdentity.UserId,
                Name = UserIdentity.Name,
                Company = UserIdentity.Company,
                Title = UserIdentity.Title,
                Avatar = UserIdentity.Avatar,
                ApplyTime = DateTime.Now
            }, cancellationToken);

            if (!result)
            {
                //log tbd
                return BadRequest();
            }

            return Ok();
        }

        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("apply-requests/{applierId}")]
        public async Task<IActionResult> ApprovalApplyRequest(int applierId, CancellationToken cancellationToken)
        {
            var result = await _contactApplyRequestRepository.ApprovalAsync(UserIdentity.UserId, applierId, cancellationToken);

            if (!result)
            {
                //log tbd
                return BadRequest();
            }

            var applier = await _userService.GetBaseUserInfoAsync(applierId);
            var userInfo = await _userService.GetBaseUserInfoAsync(UserIdentity.UserId);

            await _contactRepository.AddContactAsync(UserIdentity.UserId, applier, cancellationToken);

            await _contactRepository.AddContactAsync(applierId, userInfo, cancellationToken);
            return Ok();
        }
    }
}
