using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using User.API.Models;
using Xunit;
using Microsoft.AspNetCore.JsonPatch;
using User.API.Controllers;
using User.API.Data;
using System.Linq;
using System.Collections.Generic;

namespace User.API.UnitTests
{
    public class UserControllerUnitTests
    {
        private Data.UserContext GetUserContext()
        {
            var options = new DbContextOptionsBuilder<Data.UserContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var userContext = new Data.UserContext(options);
            userContext.Users.Add(new Models.AppUser
            {
                Id = 1,
                Name = "hobo"
            });

            userContext.SaveChanges();
            return userContext;
        }

        private (UserController controller, UserContext userContext) GetUserController()
        {
            var context = GetUserContext();
            var loggerMoq = new Mock<ILogger<Controllers.UserController>>();
            var logger = loggerMoq.Object;
            return (controller: new UserController(context, logger), userContext: context);
        }

        [Fact]
        public async Task Get_ReturnRightUser_WithExpectedParameters()
        {
            (UserController controller, UserContext userContext) = GetUserController();
            var response = await controller.Get();

            var result = response.Should().BeOfType<JsonResult>().Subject;
            var appUser = result.Value.Should().BeAssignableTo<AppUser>().Subject;
            appUser.Id.Should().Be(1);
            appUser.Name.Should().Be("hobo");
        }

        [Fact]
        public async Task Patch_ReturnNewName_WithRightExpectedNewNameParameters()
        {
            (UserController controller, UserContext userContext) = GetUserController();
            var document = new JsonPatchDocument<AppUser>();
            document.Replace(user => user.Name, "lei");
            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;

            //assert response
            var appUser = result.Value.Should().BeAssignableTo<AppUser>().Subject;
            appUser.Name.Should().Be("lei");

            //assert name value in ef context
            var userModel = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Should().NotBeNull();
            userModel.Name.Should().Be("lei");
        }

        [Fact]
        public async Task Patch_ReturnNewProperties_WithAddNewProperties()
        {
            (UserController controller, UserContext userContext) = GetUserController();
            var document = new JsonPatchDocument<AppUser>();
            document.Replace(u => u.Properties, new List<UserProperty>
            {
                new UserProperty{ Key ="fin_industry",Value="»¥ÁªÍø",Text="»¥ÁªÍø"}
            });
            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;

            //assert response
            var appUser = result.Value.Should().BeAssignableTo<AppUser>().Subject;
            appUser.Properties.Count.Should().Be(1);
            appUser.Properties.First().Value.Should().Be("»¥ÁªÍø");
            appUser.Properties.First().Key.Should().Be("fin_industry");


            //assert name value in ef context
            var userModel = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Properties.Count.Should().Be(1);
            userModel.Properties.First().Value.Should().Be("»¥ÁªÍø");
            userModel.Properties.First().Key.Should().Be("fin_industry");
        }

        [Fact]
        public async Task Patch_ReturnNewProperties_WithRemoveProperty()
        {
            (UserController controller, UserContext userContext) = GetUserController();
            var document = new JsonPatchDocument<AppUser>();
            document.Replace(u => u.Properties, new List<UserProperty>{});
            var response = await controller.Patch(document);
            var result = response.Should().BeOfType<JsonResult>().Subject;

            //assert response
            var appUser = result.Value.Should().BeAssignableTo<AppUser>().Subject;
            appUser.Properties.Should().BeEmpty();

            //assert name value in ef context
            var userModel = await userContext.Users.SingleOrDefaultAsync(u => u.Id == 1);
            userModel.Properties.Should().BeEmpty();
        }
    }
}
