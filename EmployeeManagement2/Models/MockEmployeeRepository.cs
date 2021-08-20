using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace EmployeeManagement.Models
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private List<Employee> _EmployeeList;
        public MockEmployeeRepository()
        {
            _EmployeeList = new List<Employee>() {
            new Employee() { Id = 1, Name = "Mary", Department = Dept.HR, Email = "Mary5555@gmail.com" },
            new Employee() { Id = 2, Name = "John", Department = Dept.IT, Email = "John5555@gmail.com" },
            new Employee() { Id = 3, Name = "Sam", Department = Dept.PayRoll, Email = "Sam5555@gmail.com" }
            };
        }

        public Employee add(Employee employee)
        {
            employee.Id= _EmployeeList.Max(e => e.Id)+1 ;
            _EmployeeList.Add(employee);
            return employee;
        }

        public IEnumerable<Employee> GetAllEmployees()
        {
            return _EmployeeList;
        }

        public Employee GetEmployee(int ID)
        {
            return _EmployeeList.FirstOrDefault(e => e.Id == ID);
        }
    }
       
}

