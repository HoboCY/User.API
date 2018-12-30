using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.API.Models;

namespace User.API.Data
{
    public class UserContextSeed
    {
        private ILogger<UserContextSeed> _logger;

        public UserContextSeed(ILogger<UserContextSeed> logger)
        {
            _logger = logger;
        }

        public static async Task SeedAsync(IApplicationBuilder app, ILoggerFactory loggerFactory, int? retry = 0)
        {
            var retryForAvaiability = retry.Value;


            try
            {
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var context = (UserContext)scope.ServiceProvider.GetService(typeof(UserContext));
                    var logger = (ILogger<UserContextSeed>)scope.ServiceProvider.GetService(typeof(ILogger<UserContextSeed>));
                    logger.LogDebug("Begin UserContextSeed SeedAsync");

                    context.Database.Migrate();

                    if (!context.Users.Any())
                    {
                        context.Users.Add(new AppUser { Name = "hobo" });
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                if (retryForAvaiability < 10)
                {
                    retryForAvaiability++;
                    var logger = loggerFactory.CreateLogger(typeof(UserContextSeed));
                    logger.LogError(ex.Message);
                    await SeedAsync(app, loggerFactory, retryForAvaiability);
                }
            }
        }
    }
}
