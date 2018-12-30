using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contact.API.Dtos;
using Contact.API.Options;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Resilience;

namespace Contact.API.Services
{
    public class UserService : IUserService
    {
        private IHttpClient _httpClient;
        private string _userServiceUrl;
        private ILogger<UserService> _logger;

        public UserService(IHttpClient httpClient,
            IOptions<ServiceDiscoveryOptions> serviceDiscoveryOptions,
            IDnsQuery dnsQuery,
            ILogger<UserService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var address = dnsQuery.ResolveService("service.consul",
                serviceDiscoveryOptions.Value.UserServiceName);
            var addressList = address.First().AddressList;

            var host = addressList.Any() ? addressList.First().ToString() : address.First().HostName;
            var port = address.First().Port;

            _userServiceUrl = $"http://{host}:{port}";
        }
        public async Task<UserIdentity> GetBaseUserInfoAsync(int userId)
        {
            _logger.LogTrace($"Enter into GetBaseUserInfoAsync:{userId}");
            try
            {
                var response = await _httpClient.GetStringAsync($"{_userServiceUrl}/api/users/baseUserInfo/{userId}");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var result = JsonConvert.DeserializeObject<UserIdentity>(response);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(" GetBaseUserInfoAsync 在重试之后失败", ex.Message + ex.StackTrace);
                throw ex;
            }
            return null;
        }
    }
}
