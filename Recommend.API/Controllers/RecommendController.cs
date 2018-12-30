using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recommend.API.Data;
using Recommend.API.Dtos;

namespace Recommend.API.Controllers
{
    [Route("api/[controller]")]
    public class RecommendController : BaseController
    {
        private readonly RecommendContext _recommendContext;
        public RecommendController(RecommendContext recommendContext)
        {
            _recommendContext = recommendContext;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Get()
        {
            return Ok(await _recommendContext.ProjectRecommends.AsNoTracking()
                .Where(r => r.UserId == UserIdentity.UserId).ToListAsync());
        }
    }
}