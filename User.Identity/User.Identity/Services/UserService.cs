using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Resilience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using User.Identity.Dtos;

namespace User.Identity.Services
{
    public class UserService : IUserService
    {
        private IHttpClient _httpClient;
        private string _userServiceUrl;
        private ILogger<UserService> _logger;

        public UserService(IHttpClient httpClient,
            IOptions<Options.ServiceDiscoveryOptions> serviceDiscoveryOptions,
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

        public async Task<Dtos.UserInfo> CheckOrCreate(string phone)
        {
            _logger.LogTrace($"Enter into CheckOrCreate:{phone}");
            var form = new Dictionary<string, string> { { "phone", phone } };

            try
            {
                var response = await _httpClient.PostAsync(string.Concat(_userServiceUrl,
                "/api/users/check-or-create"), form);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonConvert.DeserializeObject<UserInfo>(result);
                    _logger.LogTrace($"Completed CheckOrCreate with userId:{userInfo.Id}");
                    return userInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(" CheckOrCreate 在重试之后失败", ex.Message + ex.StackTrace);
                throw ex;
            }
            return null;
        }
    }
}
