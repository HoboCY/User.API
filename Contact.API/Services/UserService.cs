using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contact.API.Dtos;

namespace Contact.API.Services
{
    public class UserService : IUserService
    {
        public async Task<BaseUserInfo> GetBaseUserInfoAsync(int userId)
        {
            return new BaseUserInfo();
        }
    }
}
