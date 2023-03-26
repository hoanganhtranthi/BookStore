using BookStore.Data.Models;
using NTQ.Sdk.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTQ.Sdk.Core.Attributes;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Service.DTO.Response
{
    public class UserResponse : SortModel
    {
        [Key]
        public int UserId { get; set; }
        [StringAttribute]
        public string? UserName { get; set; }
        [StringAttribute]
        public string? Address { get; set; }
        [SkipAttribute]
        public DateTime DateOfBirth { get; set; }
        [StringAttribute]
        public string? Gender { get; set; }
        [StringAttribute]
        public string? Password { get; set; }
        [StringAttribute]
        public string? PasswordResetToken { get; set; }
        [StringAttribute]
        public string? Email { get; set; }
    }
}
