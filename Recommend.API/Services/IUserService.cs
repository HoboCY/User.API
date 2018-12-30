using Recommend.API.Dtos;
using System.Threading.Tasks;

namespace Recommend.API.Services
{
    public interface IUserService
    {
        /// <summary>
        /// 获取用户的基本信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserIdentity> GetBaseUserInfoAsync(int userId);
    }
}
