using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Utilities
{
    public class ValidEmailDomainAttribute : ValidationAttribute
    {
        private readonly string allowedDoamin;

        public ValidEmailDomainAttribute(string allowedDomain)
        {
            this.allowedDoamin = allowedDomain;
        }


        public override bool IsValid(object value)
        {

            string[] Email = value.ToString().Split("@");
            return (Email[1].ToUpper() == allowedDoamin.ToUpper());



        }
    }
}
