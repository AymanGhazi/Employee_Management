using EmployeeManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModel
{
    public class HomeDetailsVM
    {
        public Employee Employee { get; set; }
        public string PageTitle { get; set; }
    }
}
