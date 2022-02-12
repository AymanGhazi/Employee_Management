using EmployeeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Models
{
    public class SQLEmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext context;

        public SQLEmployeeRepository(AppDbContext Context)
        {
            context = Context;
        }

        public Employee Add(Employee employee)
        {
            context.Add(employee);
            context.SaveChanges();
            return employee;
        }

        public Employee Delete(int id)
        {
            Employee removedEmployee = context.Employees.Find(id);
            if (removedEmployee != null)
            {
                context.Remove(removedEmployee);
                context.SaveChanges();
            }

            return removedEmployee;
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return context.Employees;
        }

        public Employee GetEmployee(int Id)
        {
            return context.Employees.Find(Id);
        }

        public Employee Update(Employee EmployeeChanges)
        {
            var updatesEmployee = context.Employees.Attach(EmployeeChanges);
            updatesEmployee.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            context.SaveChanges();
            return EmployeeChanges;
        }
    }
}
