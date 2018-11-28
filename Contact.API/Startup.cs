using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Contact.API.Data;
using Contact.API.Infrastructure;
using Contact.API.Models;
using Contact.API.Options;
using Contact.API.Services;
using DnsClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;

namespace Contact.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(a =>
            {
                a.MongoConnectionString = Configuration["MongoConnectionString"].ToString();
                a.MongoContactDatabase = Configuration["MongoContactDatabase"].ToString();
            });
            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<IDnsQuery>(d =>
            {
                var serviceConfiguration = d.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                return new LookupClient(IPAddress.Parse(serviceConfiguration.Consul.DnsEndpoint.Address), serviceConfiguration.Consul.DnsEndpoint.Port);
            });
            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<IConsulClient>(s => new ConsulClient(cfg =>
            {
                var servicesConfiguration = s.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                if (!string.IsNullOrEmpty(servicesConfiguration.Consul.HttpEndpoint))
                {
                    cfg.Address = new Uri(servicesConfiguration.Consul.HttpEndpoint);
                }
            }));

            services.AddSingleton(sp =>
            {
                var loggger = sp.GetRequiredService<ILogger<ResilienceHttpClient>>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var retryCount = 5;
                var expCountAllowedBeforeBreak = 5;
                var factory = new ResilienceClientFactory(loggger, httpContextAccessor, retryCount, expCountAllowedBeforeBreak);
                return factory;
            });

            services.AddSingleton<IHttpClient>(sp =>
            {
                var resilienceClientFactory = sp.GetRequiredService<ResilienceClientFactory>();
                return resilienceClientFactory.GetResilienceHttpClient();
            });


            services.AddSingleton<ContactContext>();
            services.AddScoped<IContactRepository, MongoContactRepository>()
                .AddScoped<IContactApplyRequestRepository, MongoContactApplyRequestRepository>()
                .AddScoped<IUserService, UserService>();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = "contact_api";
                    options.Authority = "http://localhost:8070";    //网关地址
                });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime lifetime,
            IOptions<ServiceDiscoveryOptions> options,
            IConsulClient consulClient)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            lifetime.ApplicationStarted.Register(() =>
            {
                RegisterService(app, options, consulClient);
            });

            lifetime.ApplicationStopped.Register(() =>
            {
                DeRegisterService(app, options, consulClient);
            });

            app.UseAuthentication();
            app.UseMvc();
        }

        private void DeRegisterService(IApplicationBuilder app, IOptions<ServiceDiscoveryOptions> serviceOptions, IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                consul.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
            }
        }

        private void RegisterService(IApplicationBuilder app,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                var httpCheck = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                    Interval = TimeSpan.FromSeconds(30),
                    HTTP = new Uri(address, "HealthCheck").OriginalString
                };

                var registration = new AgentServiceRegistration()
                {
                    Checks = new[] { httpCheck },
                    Address = address.Host,
                    ID = serviceId,
                    Name = serviceOptions.Value.ServiceName,
                    Port = address.Port
                };

                consul.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
            }
        }
    }
}
