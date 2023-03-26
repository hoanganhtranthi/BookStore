using BookStore.Data.Models;
using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using Microsoft.AspNetCore.Mvc;
using NTQ.Sdk.Core.CustomModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.Service.InterfaceService
{
    public interface IAuthUserService
    {
        bool IsUniqueUser(string Email);
        Task<BaseResponseViewModel<LoginResponse>> Login(LoginRequest loginRequest,string emailLogin, ResetPasswordRequest request,EmailRequest emailRequest, bool forgotPass);
        Task<BaseResponseViewModel<UserResponse>> Registeration(RegisterationRequest registerationRequest);
    }
}
