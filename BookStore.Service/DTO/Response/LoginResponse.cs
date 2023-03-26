using BookStore.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.DTO.Response
{
    public class LoginResponse
    {
        public UserResponse User { get; set; }
        public string Token { get; set; }
    }
}
