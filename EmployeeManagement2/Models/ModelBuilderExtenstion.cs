using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public static class ModelBuilderExtenstion
    {
        public static void seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasData(new Employee
            {
                Id = 1,
                Name = "Mark",
                Department = Dept.HR,
                Email = "Mark@gmail.com",
            }, new Employee
            {
                Id = 2,
                Name = "Mary",
                Department = Dept.HR,
                Email = "Mary@gmail.com",
            }
            );
        }
    }
}
