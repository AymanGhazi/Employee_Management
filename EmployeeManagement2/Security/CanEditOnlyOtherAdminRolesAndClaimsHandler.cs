using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeeManagement.Security
{
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaims>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context
            , ManageAdminRolesAndClaims requirement)
        {
            var authFilterContext = context.Resource as AuthorizationFilterContext;
            //is not going to protect any method as AuthorizationHandlerContext returns methods
            if (authFilterContext == null)
            {
                return Task.CompletedTask;
            }
            string LoggedInAdminId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            string adminBeingEdited = authFilterContext.HttpContext.Request.Query["userId"];


            if (context.User.IsInRole("admin") &&
                context.User.HasClaim(claim => claim.Type == "Edit Role" && claim.Value == "true")
                && adminBeingEdited.ToLower() != LoggedInAdminId.ToLower())
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
