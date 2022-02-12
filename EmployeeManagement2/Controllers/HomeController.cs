using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModel;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using EmployeeManagement.Security;

namespace EmployeeManagement.Controllers
{

    public class HomeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IHostingEnvironment hostingEnvironment;
        private readonly ILogger<HomeController> logger;

        private readonly IDataProtector protector;

        public HomeController(IEmployeeRepository employeeRepository,
            IHostingEnvironment hostingEnvironment, ILogger<HomeController> logger,
            IDataProtectionProvider dataProtectionProvider, DataProtectionPurposeString dataProtectionPurposeString)
        {
            _employeeRepository = employeeRepository;
            this.hostingEnvironment = hostingEnvironment;
            this.logger = logger;
            protector = dataProtectionProvider.CreateProtector(dataProtectionPurposeString.employeeIdRouteValue);
        }

        [AllowAnonymous]
        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees().Select(
                e =>
                {
                    e.EncyptedId = protector.Protect(e.Id.ToString());
                    return e;
                });
            return View(model);
        }
        [AllowAnonymous]
        public ViewResult Details(string id)
        {

            // throw new Exception("Error in Details view ");
            logger.LogTrace("trace log");
            logger.LogDebug("Debug  log");
            logger.LogInformation("Inf log");
            logger.LogWarning("Warning log");
            logger.LogError("Error log");
            logger.LogCritical("Critical log");
            int EmployeeId = Convert.ToInt32(protector.Unprotect(id));

            Employee employee = _employeeRepository.GetEmployee(EmployeeId);
            if (employee == null)
            {
                Response.StatusCode = 404;
                return View("EmployeeNotFound", EmployeeId);
            }

            HomeDetailsVM homeDetailsVM = new HomeDetailsVM()
            {
                Employee = employee,
                PageTitle = "Employee Details"

            };
            return View(homeDetailsVM);
        }
        [HttpGet]

        public ViewResult Create()
        {

            return View();
        }
        [HttpGet]

        public ViewResult Edit(int id)
        {
            Employee employee = _employeeRepository.GetEmployee(id);
            EmployeeEditViewModel employeeEditModel = new EmployeeEditViewModel
            {
                Id = employee.Id,
                Name = employee.Name,
                Department = employee.Department,
                Email = employee.Email,
                ExitingPhotoPath = employee.PhotoPath
            };



            return View(employeeEditModel);
        }

        [HttpPost]
        public IActionResult Edit(EmployeeEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Employee employee = _employeeRepository.GetEmployee(model.Id);
                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.Department = model.Department;
                if (model.Photo != null)
                {
                    //delete the old photo
                    if (model.ExitingPhotoPath != null)
                    {
                        string photopath = Path.Combine(hostingEnvironment.WebRootPath, "images", model.ExitingPhotoPath);
                        System.IO.File.Delete(photopath);
                    }
                    employee.PhotoPath = ProcessUploadedFile(model);
                }



                _employeeRepository.Update(employee);
                return RedirectToAction("details", new { id = employee.Id });
            }

            return View();
        }

        private string ProcessUploadedFile(EmployeeCreateViewModel model)
        {
            string uniqueFileName = null;
            // If the Photo property on the incoming model object is not null, then the user
            // has selected an image to upload.
            if (model.Photo != null)
            {
                // The image must be uploaded to the images folder in wwwroot
                // To get the path of the wwwroot folder we are using the inject
                // HostingEnvironment service provided by ASP.NET Core
                string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
                // To make sure the file name is unique we are appending a new
                // GUID value and and an underscore to the file name
                uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                // Use CopyTo() method provided by IFormFile interface to
                // copy the file to wwwroot/images folder
                using (var filestream = new FileStream(filePath, FileMode.Create))
                {
                    model.Photo.CopyTo(filestream);
                }

            }

            return uniqueFileName;
        }

        [HttpPost]
        public IActionResult Create(EmployeeCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = ProcessUploadedFile(model);



                Employee newEmployee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    Department = model.Department,
                    // Store the file name in PhotoPath property of the employee object
                    // which gets saved to the Employees database table
                    PhotoPath = uniqueFileName
                };

                _employeeRepository.Add(newEmployee);
                return RedirectToAction("details", new { id = newEmployee.Id });
            }

            return View();
        }

        public ViewResult Delete(int id)
        {
            Employee employee = _employeeRepository.Delete(id);

            return View("Index");
        }
    }
}