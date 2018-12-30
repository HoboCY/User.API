using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Project.Infrastructure;

namespace Project.API.Applications.Queries
{
    public class ProjectQueries : IProjectQueries
    {
        private ProjectContext _projectContext;

        public ProjectQueries(ProjectContext projectContext)
        {
            _projectContext = projectContext;
        }

        public async Task<dynamic> GetProjectDetail(int projectId)
        {
            var sql = @"SELECT 
            Projects.Company,
            Projects.City,
            Projects.AreaName,
            Projects.Province,
            Projects.FinStage,
            Projects.FinMoney,
            Projects.Valuation,
            Project.FinPercentage,
            Projects.Introduction,
            Projects.UserId,
            Projects.Income,
            Projects.Revenue,
            Projects.UserName,
            Projects.Avatar,
            Projects.BrokerageOptions,
            ProjectVisibleRules.Tags,
            ProjectVisibleRules.Visible
            FROM Projects a INNER JOIN ProjectVisibleRules b ON Projects.Id = ProjectVisibleRules.ProjectId
            WHERE Projects.Id = @projectId";
            using (var conn = _projectContext.Database.GetDbConnection())
            {
                conn.Open();
                var result = await conn.QueryAsync<dynamic>(sql, new { projectId });
                return result;
            }
        }

        public async Task<dynamic> GetProjectsByUserId(int userId)
        {
            var sql = @"SELECT 
            Projects.Id,Projects.Avatar,Projects.Company,Projects.FinStage,Projects.Introduction,Projects.Tags,
            Projects.ShowSecurityInfo,Projects.CreatedTime FROM Projects
            WHERE Projects.UserId = @userId";
            using (var conn = _projectContext.Database.GetDbConnection())
            {
                conn.Open();
                var result = await conn.QueryAsync<dynamic>(sql, new { userId });
                return result;
            }
        }
    }
}
