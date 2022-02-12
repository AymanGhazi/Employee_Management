using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EmployeeManagement.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ILogger logger;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
        }
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            returnUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properities = signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);


            return new ChallengeResult(provider, properities);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            LoginViewModel model = new LoginViewModel
            {
                ReturnURL = returnUrl,
                ExternlLogIns = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };
            if (remoteError != null)
            {
                ModelState.AddModelError("", $"Error Hppen While get informtion from provider{remoteError}");
                return View("Login", model);
            }

            var Info = await signInManager.GetExternalLoginInfoAsync();
            if (Info == null)
            {
                ModelState.AddModelError("", $"Error Happen While get informtion from provider");
                return View("Login", model);
            }

            var email = Info.Principal.FindFirstValue(ClaimTypes.Email);
            ApplicationUser user = null;
            if (email != null)
            {
                user = await userManager.FindByEmailAsync(email);

                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Email is not confirmed ");
                    return View("Login", model);
                }

                var LogInExternalResult = await signInManager.ExternalLoginSignInAsync(Info.LoginProvider, Info.ProviderKey, false, true);
                if (LogInExternalResult.Succeeded)
                {
                    return LocalRedirect(returnUrl);
                }
                else
                {

                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = Info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = Info.Principal.FindFirstValue(ClaimTypes.Email)
                        };
                        var result = await userManager.CreateAsync(user);
                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                        var confirmtionLink = Url.Action("ConfirmEmail", "Account",
                       new { userId = user.Id, token = token }, Request.Scheme);

                        logger.Log(LogLevel.Warning, confirmtionLink);

                        ViewBag.ErrorTitle = "Registration successful";
                        ViewBag.ErrorMessage = "Before you can Login, please confirm your " +
                                "email, by clicking on the confirmation link we have emailed you";
                        return View("Error");
                    }
                    await userManager.AddLoginAsync(user, Info);
                    await signInManager.SignInAsync(user, false);
                    return LocalRedirect(returnUrl);
                }


            }
            ViewBag.ErrorTitle = $"Email claim not received from: {Info.LoginProvider}";
            ViewBag.ErrorMessage = "Please contact support on Pragim@PragimTech.com";

            return View("Error");

        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Copy data from RegisterViewModel to IdentityUser
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    City = model.City,
                };


                // Store user data in AspNetUsers database table
                var result = await userManager.CreateAsync(user, model.Password);

                // If user is successfully created, sign-in the user using
                // SignInManager and redirect to index action of HomeController
                if (result.Succeeded)
                {
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    var confirmtionLink = Url.Action("ConfirmEmail", "Account",
                   new { userId = user.Id, token = token }, Request.Scheme);

                    logger.Log(LogLevel.Warning, confirmtionLink);

                    if (signInManager.IsSignedIn(User) && User.IsInRole("admin"))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }
                    ViewBag.ErrorTitle = "Registration successful";
                    ViewBag.ErrorMessage = "Before you can Login, please confirm your " +
                            "email, by clicking on the confirmation link we have emailed you";
                    return View("Error");
                }

                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {

            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "home");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"the user Id {userId} is not found";
                return View("NotFound");

            }
            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View();
            }
            ViewBag.ErrorTitle = "Email cannot be confirmed";
            return View("Error");


        }

        [AcceptVerbs("Post", "Get")]
        [AllowAnonymous]
        public async Task<JsonResult> IsEmailInUse(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(true); //jquery validate returns ajax
            }
            return Json($"Email: {email} is already in use");

        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string ReturnURL)
        {
            LoginViewModel model = new LoginViewModel
            {

                ExternlLogIns = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
                ReturnURL = ReturnURL,
            };
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            model.ExternlLogIns = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: model.RememberMe, lockoutOnFailure: true);
                if (ModelState.IsValid)
                {
                    var user = await userManager.FindByEmailAsync(model.Email);
                    if (user != null && !user.EmailConfirmed && (await userManager.CheckPasswordAsync(user, model.Password)))
                    {
                        ModelState.AddModelError(string.Empty, "Email is not confirmed ");
                        return View(model);
                    }

                    if (result.Succeeded)
                    {
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return LocalRedirect(returnUrl);// redirect vulernability
                        }
                        else
                        {
                            return RedirectToAction("index", "home");
                        }
                    }
                    if (result.IsLockedOut)
                    {
                        return View("AccountLocked");
                    }


                    else ModelState.AddModelError(string.Empty, "Invalid Login Attempt ");
                }
                return View(model);
            }
            return View(model);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {


            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (ModelState.IsValid)
            {
                if (user != null && await userManager.IsEmailConfirmedAsync(user))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var PasswordResetLink = Url.Action("ResetPassword",
                        "Account", new { email = model.Email, token = token }, Request.Scheme);


                    logger.Log(LogLevel.Error, PasswordResetLink);

                    return View("ForgetPasswordConfirmation");
                }
                return View("ForgetPasswordConfirmation");

            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            if (token == null && email == null)
            {
                ModelState.AddModelError("", "In valid Password Reset Token...");
            }



            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        if (await userManager.IsLockedOutAsync(user))
                        {
                            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                        }
                        return View("ResetPasswordConfirmation");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);

                }
                return View("ResetPasswordConfirmation");

            }

            return View(model);

        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await userManager.GetUserAsync(User);
            var UserHasPassword = await userManager.HasPasswordAsync(user);

            if (!UserHasPassword)
            {
                return RedirectToAction("AddPassword");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);

                if (user == null)
                {
                    return RedirectToAction("Login");
                }
                var changePasswordResult = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                await signInManager.RefreshSignInAsync(user);
                return View("ChangedPassword");
            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> AddPassword()
        {
            var user = await userManager.GetUserAsync(User);
            var UserHasPassword = await userManager.HasPasswordAsync(user);

            if (UserHasPassword)
            {
                return RedirectToAction("ChangePassword");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            var result = await userManager.AddPasswordAsync(user, model.NewPassword);




            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View();

            }
            await signInManager.RefreshSignInAsync(user);
            return View("AddedPassword");
        }

    }
}