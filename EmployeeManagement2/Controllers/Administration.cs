using EmployeeManagement.Models;
using EmployeeManagement.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeeManagement.Controllers
{
    // [Authorize(Policy = "AdminRolePolicy")]
    public class Administration : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<Administration> logger;

        public Administration(RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<Administration> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }
        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                return View("NotFound");
            }

            // UserManager service GetClaimsAsync method gets all the current claims of the user
            var existingUserClaims = await userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel
            {
                UserId = userId
            };

            // Loop through each claim we have in our application
            foreach (Claim claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type
                };

                // If the user has the claim, set IsSelected property to true, so the checkbox
                // next to the claim is checked on the UI
                if (existingUserClaims.Any(c => c.Type == claim.Type && c.Value == "true"))
                {
                    userClaim.IsSelected = true;
                }

                model.Claims.Add(userClaim);
            }

            return View(model);

        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} cannot be found";
                return View("NotFound");
            }

            // Get all the user existing claims and delete them
            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing claims");
                return View(model);
            }

            // Add all the claims that are selected on the UI
            result = await userManager.AddClaimsAsync(user,
                model.Claims.Select(c => new Claim(c.ClaimType, c.IsSelected ? "true" : "false")));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected claims to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = model.UserId });

        }


        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"this user with id {userId} Not Found";
                return View("NotFound", "Error");
            }
            var model = new List<UserRolesViewModel>();
            foreach (var role in roleManager.Roles)
            {
                var userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    Rolename = role.Name

                };
                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.IsSelected = true;
                }
                else
                {
                    userRolesViewModel.IsSelected = false;
                }
                model.Add(userRolesViewModel);
            }

            return View(model);
        }
        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(List<UserRolesViewModel> model, string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"this user with id = {userId} Not Found";
                return View("NotFound", "Error");
            }
            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Can not Remove User Existing Roles");
                return View(model);

            }
            result = await userManager.AddToRolesAsync(user, model.Where(x => x.IsSelected).Select(y => y.Rolename));

            if (!result.Succeeded)

            {
                ModelState.AddModelError("", "Cannot add ths role to the user");
                return View(model);
            }


            return RedirectToAction("EditUser", new { id = userId });
        }





        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = userManager.Users;
            return View(users);
        }

        [HttpGet]

        public IActionResult CreateRole()
        {

            return View();
        }
        [HttpPost]

        public async Task<IActionResult> CreateRole(CreateRoleModel model)
        {

            if (ModelState.IsValid)
            {
                IdentityRole role = new IdentityRole
                {
                    Name = model.RoleName
                };
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles", "Administration");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }
        [HttpGet]

        public async Task<IActionResult> EditRole(string Id)
        {
            var role = await roleManager.FindByIdAsync(Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"this role with id {Id} Not Found";
                return View("NotFound", "Error");
            }
            else
            {
                var model = new EditRoleViewModel
                {
                    Id = role.Id,
                    RoleName = role.Name
                };
                foreach (var user in userManager.Users)
                {
                    if (await userManager.IsInRoleAsync(user, role.Name))
                    {
                        model.users.Add(user.UserName);
                    }
                }
                return View(model);
            }
        }
        [HttpPost]


        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"this role with id {model.Id} Not Found";
                return View("NotFound", "Error");
            }
            else
            {
                role.Name = model.RoleName;
                var result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                }

            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string RoleId)
        {
            ViewBag.roleId = RoleId;
            var Role = await roleManager.FindByIdAsync(RoleId);
            if (Role == null)
            {
                ViewBag.ErrorMessage = $"This Role With ID {RoleId} IS not found";
                return View("NotFound");
            }
            var model = new List<UserRoleViewModel>();
            foreach (var user in userManager.Users)
            {
                var userRoleModelView = new UserRoleViewModel
                {
                    userId = user.Id,
                    UserName = user.UserName
                };
                if (await userManager.IsInRoleAsync(user, Role.Name))
                {
                    userRoleModelView.IsSelected = true;
                }
                else
                {
                    userRoleModelView.IsSelected = false;
                }
                model.Add(userRoleModelView);
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"this Role With Id {roleId} is not found";
                return View("NotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].userId);
                IdentityResult result = null;
                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }
                if (result.Succeeded)
                {
                    if (i < model.Count - 1)
                    {
                        continue;
                    }
                    return RedirectToAction("EditRole", new { id = role.Id });
                }
            }
            return RedirectToAction("EditRole", new { id = role.Id });
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var User = await userManager.FindByIdAsync(id);
            if (User == null)
            {
                ViewBag.ErrorMessage = $"this Role With Id {id} is not found";
                return View("NotFound");
            }
            var UserClaims = await userManager.GetClaimsAsync(User);
            var userRoles = await userManager.GetRolesAsync(User);
            var model = new EditUserViewModel
            {
                Id = User.Id,
                UserName = User.UserName,
                Email = User.Email,
                City = User.City,
                Claims = UserClaims.Select(e => e.Type + " : " + e.Value).ToList(),
                Roles = userRoles
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var User = await userManager.FindByIdAsync(model.Id);
            if (User == null)
            {
                ViewBag.ErrorMessage = $"this Role With Id {model.Id} is not found";
            }
            else
            {
                User.Email = model.Email;
                User.UserName = model.UserName;
                User.City = model.City;
                var result = await userManager.UpdateAsync(User);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            return View(model);
        }

        public async Task<IActionResult> DeleteUser(string Id)
        {
            var user = await userManager.FindByIdAsync(Id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"this Role With Id {Id} is not found";
                return View("NotFound");
            }
            try
            {
                var result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListUsers");
            }
            catch (DbUpdateException ex)

            {
                logger.LogError($"Delete User In Role : {ex}");
                ViewBag.ErrorTitle = $"this user {user.Email} is in Role";
                ViewBag.ErrorMessage = $"{user.Email} role cannot be deleted as there are Roles in this User. " +
                    $"If you want to delete this User, please remove the Role from the User and then try to delete";

                return View("Error");
            }

        }
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string Id)
        {

            var role = await roleManager.FindByIdAsync(Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"this Role With Id {Id} is not found";
                return View("NotFound");
            }
            try
            {

                var result = await roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View("ListRoles");
            }
            catch (DbUpdateException ex)
            {
                logger.LogError($"Error Deleting Role  In Use{ex}");

                ViewBag.ErrorTitle = $"this Role {role.Name} is in Use";
                ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted as there are users in this role. " +
                    $"If you want to delete this role, please remove the users from the role and then try to delete";
                return View("Error");
            }

        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


    }
}


