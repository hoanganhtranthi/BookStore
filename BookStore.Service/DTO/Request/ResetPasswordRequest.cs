using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ServiceStack.LicenseUtils;

namespace BookStore.Service.DTO.Request
{
    public class ResetPasswordRequest
    {
        [ MinLength(6, ErrorMessage = "Please enter at least 6 characters !")]
        public string? Password { get; set; }
        [Compare("Password")]
        public string? ConfirmPassword { get; set; }

        public string? PasswordResetToken { get; set; }
    }
}
