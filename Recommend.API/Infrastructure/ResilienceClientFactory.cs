using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Resilience;

namespace Recommend.API.Infrastructure
{
    public class ResilienceClientFactory
    {
        private ILogger<ResilienceHttpClient> _logger;
        private IHttpContextAccessor _httpContextAccessor;

        //重试次数
        private int _retryCount;

        //熔断之前允许的异常次数
        private int _exceptionCountAllowedBeforeBreaking;

        public ResilienceClientFactory(ILogger<ResilienceHttpClient> logger,
            IHttpContextAccessor httpContextAccessor,
            int retryCount,
            int exceptionCountAllowedBeforeBreaking)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _retryCount = retryCount;
            _exceptionCountAllowedBeforeBreaking = exceptionCountAllowedBeforeBreaking;
        }

        public ResilienceHttpClient GetResilienceHttpClient() =>
            new ResilienceHttpClient(origin => CreatePolicy(origin), _logger, _httpContextAccessor);

        private Policy[] CreatePolicy(string origin) => new Policy[]
        {
                Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(_retryCount,
                    retryAttempt=>TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception,timeSpan,retryCount,context)=>
                    {
                        var msg= $"第 {retryCount} 次重试 "+
                                $"of {context.PolicyKey} "+
                                $"at {context.ExecutionKey}, "+
                                $"due to：{exception}. ";
                        _logger.LogWarning(msg);
                        _logger.LogDebug(msg);
                    }),
                Policy.Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    _exceptionCountAllowedBeforeBreaking,
                    TimeSpan.FromMinutes(1), //熔断持续时间
                    (exception,duration)=>
                    {
                        _logger.LogTrace("熔断器打开");
                    },()=>{
                         _logger.LogTrace("熔断器关闭");
                    })
            };
    }
}
