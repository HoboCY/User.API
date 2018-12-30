using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Project.Domain.AggregatesModel;
using Project.API.Applications.Commands;
using Project.API.Applications.Service;
using Project.API.Applications.Queries;

namespace Project.API.Controllers
{
    [Route("api/[controller]")]
    public class ProjectController : BaseController
    {
        private IMediator _mediator;
        private IRecommendService _recommendService;
        private IProjectQueries _projectQueries;
        public ProjectController(IMediator mediator,
            IRecommendService recommendService,
            IProjectQueries projectQueries)
        {
            _mediator = mediator;
            _recommendService = recommendService;
            _projectQueries = projectQueries;
        }

        /// <summary>
        /// 查询当前用户的项目列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetProjects()
        {
            var projects = await _projectQueries.GetProjectsByUserId(UserIdentity.UserId);
            return Ok(projects);
        }

        /// <summary>
        /// 获取当前项目详情
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("my/{projectId}")]
        public async Task<IActionResult> GetMyProjectDetail(int projectId)
        {
            var project = await _projectQueries.GetProjectDetail(projectId);
            if (project.UserId == UserIdentity.UserId)
            {
                return Ok(project);
            }
            else
            {
                return BadRequest("无权查看该项目");
            }
        }

        /// <summary>
        /// 获取推荐项目详情
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("recommend/{projectId}")]
        public async Task<IActionResult> GetRecommendProjectDetail(int projectId)
        {
            if (await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId))
            {
                var project = await _projectQueries.GetProjectDetail(projectId);
                return Ok(project);
            }
            else
            {
                return BadRequest("无权查看该项目");
            }
        }

        /// <summary>
        /// 创建项目
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateProject([FromBody]Project.Domain.AggregatesModel.Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            project.UserId = UserIdentity.UserId;
            var command = new CreateProjectCommand { Project = project };
            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// 查看项目
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("view/{projectId}")]
        public async Task<IActionResult> ViewProject(int projectId)
        {
            if (await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId))
            {
                return BadRequest("没有查看该项目的权限");
            }
            var command = new ViewProjectCommand
            {
                UserId = UserIdentity.UserId,
                UserName = UserIdentity.Name,
                Avatar = UserIdentity.Avatar,
                ProjectId = projectId
            };
            await _mediator.Send(command);
            return Ok();
        }

        /// <summary>
        /// 加入项目
        /// </summary>
        /// <param name="contributor"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("join/{projectId}")]
        public async Task<IActionResult> JoinProject(int projectId, [FromBody]ProjectContributor contributor)
        {
            if (await _recommendService.IsProjectInRecommend(projectId, UserIdentity.UserId))
            {
                return BadRequest("没有查看该项目的权限");
            }
            var command = new JoinProjectCommand { Contributor = contributor };
            await _mediator.Send(command);
            return Ok();
        }
    }
}