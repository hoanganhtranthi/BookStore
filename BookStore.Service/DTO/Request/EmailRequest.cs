using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.DTO.Request
{
    public class EmailRequest
    {
        public string EmailVerified { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
