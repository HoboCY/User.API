using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using User.API.Dtos;

namespace User.API.Controllers
{
    public class BaseController : Controller
    {
        protected UserIdentity UserIdentity
        {
            get
            {
                var identity = new UserIdentity();

                identity.UserId = int.Parse(User.Claims.First(x => x.Type == "sub").Value);
                identity.Avatar = User.Claims.First(x => x.Type == "avatar").Value;
                identity.Company = User.Claims.First(x => x.Type == "company").Value;
                identity.Name = User.Claims.First(x => x.Type == "name").Value;
                identity.Phone = User.Claims.First(x => x.Type == "phone").Value;
                identity.Title = User.Claims.First(x => x.Type == "title").Value;
                return identity;
            }
        }

    }
}