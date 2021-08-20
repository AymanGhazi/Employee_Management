﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Models;
using EmployeeManagement.ViewModel;

namespace EmployeeManagement.Controllers
{
   
    public class HomeController:Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public HomeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }
     

        public ViewResult Index()
        {
            var model = _employeeRepository.GetAllEmployees();
            return View(model);
        }

       
       

        public ViewResult Details(int? id)
        {
            HomeDetailsVM homeDetailsVM = new HomeDetailsVM()
            {
                Employee = _employeeRepository.GetEmployee(id??1),
                PageTitle = "Employee Details"

            };
            return View(homeDetailsVM);
        }
        [HttpGet]
        public ViewResult Create()
        {

            return View();
        }
        [HttpPost]
        public IActionResult Create(Employee employee)
        {
            if (ModelState.IsValid)
            {
                Employee newEmployee = _employeeRepository.add(employee);
               // return RedirectToAction("details", new { id = newEmployee.Id });
            }
             return View();
        }
    }
}