using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RequiredAttribute = ServiceStack.DataAnnotations.RequiredAttribute;

namespace BookStore.Service.DTO.Request
{
    public class RegisterationRequest
    {
        public string UserName { get; set; }

        public string Address { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; }
        [MinLength(6,ErrorMessage ="Please enter at least 6 characters !")]
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }


    }
}
