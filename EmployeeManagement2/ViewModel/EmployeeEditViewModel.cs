using EmployeeManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModel
{
    public class EmployeeEditViewModel :EmployeeCreateViewModel
    {
        public int Id { get; set; }
        public string  ExitingPhotoPath { get; set; }
    }
}
