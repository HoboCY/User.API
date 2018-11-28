using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.Identity.Authentication
{
    public class ProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectId = context.Subject.GetSubjectId() ?? throw new ArgumentNullException(nameof(context.Subject));

            if (!int.TryParse(subjectId, out int intUserID))
                throw new ArgumentException("Invalid subject identifier");

            context.IssuedClaims = context.Subject.Claims.ToList();
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            //var subject = context.Subject ?? throw new ArgumentException(nameof(context.Subject));
            //var subjectId = subject.Claims.Where(x => x.Type == "sub").FirstOrDefault().Value;
            var subjectId = context.Subject.GetSubjectId() ?? throw new ArgumentNullException(nameof(context.Subject));
            context.IsActive = int.TryParse(subjectId, out int intUserID);

            return Task.CompletedTask;
        }
    }
}
