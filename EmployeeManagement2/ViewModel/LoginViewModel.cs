using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.ViewModel
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public String Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
        public string ReturnURL { get; set; }
        public IList<AuthenticationScheme> ExternlLogIns { get; set; }
    }
}
