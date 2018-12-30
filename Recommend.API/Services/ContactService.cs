using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Recommend.API.Dtos;
using Recommend.API.Options;
using Resilience;

namespace Recommend.API.Services
{
    public class ContactService : IContactService
    {
        private IHttpClient _httpClient;
        private string _contactServiceUrl;
        private ILogger<ContactService> _logger;

        public ContactService(IHttpClient httpClient,
             IOptions<ServiceDiscoveryOptions> serviceDiscoveryOptions,
            IDnsQuery dnsQuery,
            ILogger<ContactService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            var address = dnsQuery.ResolveService("service.consul",
                serviceDiscoveryOptions.Value.UserServiceName);
            var addressList = address.First().AddressList;

            var host = addressList.Any() ? addressList.First().ToString() : address.First().HostName;
            var port = address.First().Port;

            _contactServiceUrl = $"http://{host}:{port}";
        }

        public async Task<List<Contact>> GetContactsByUserIdAsync(int userId)
        {
            _logger.LogTrace($"Enter into GetContactsByUserId:{userId}");
            try
            {
                var response = await _httpClient.GetStringAsync($"{_contactServiceUrl}/api/contacts/{userId}");
                if (!string.IsNullOrWhiteSpace(response))
                {
                    var result = JsonConvert.DeserializeObject<List<Contact>>(response);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(" GetContactsByUserIdAsync 在重试之后失败", ex.Message + ex.StackTrace);
                throw ex;
            }
            return null;
        }
    }
}
