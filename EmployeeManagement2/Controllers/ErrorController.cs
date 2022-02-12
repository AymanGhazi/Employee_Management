using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EmployeeManagement.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        // Inject ASP.NET Core ILogger service. Specify the Controller
        // Type as the generic parameter. This helps us identify later
        // which class or controller has logged the exception

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }



        // GET: /<controller>/
        [Route("Error/{statusCode}")]
        public IActionResult NotFound(int statusCode)
        {
            var statuseResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();



            ViewBag.ErrorMessage = "Sorry, the resource could not be found";
            // LogWarning() method logs the message under
            // Warning category in the log
            _logger.LogWarning($"404 error occured. Path = " +
                $"{statuseResult.OriginalPath} and QueryString = " +
                $"{statuseResult.OriginalQueryString}");


            return View("NotFound");
        }

        [AllowAnonymous]
        [Route("Error")]
        public IActionResult Error()
        {
            // Retrieve the exception Details
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            // LogError() method logs the exception under Error category in the log
            _logger.LogError($"The path {exceptionHandlerPathFeature.Path} " +
                $"threw an exception {exceptionHandlerPathFeature.Error}");

            return View("Error");
        }

    }
}
