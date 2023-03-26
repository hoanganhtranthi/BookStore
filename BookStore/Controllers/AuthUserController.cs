using AutoMapper;
using BookStore.Data.UnitOfWork;
using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using BookStore.Service.Service.InterfaceService;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;

namespace BookStore.API.Controllers
{
    [Route("api/usersauth")]
    [ApiController]
    public class AuthUserController : ControllerBase
    {
        private readonly IAuthUserService _authUserService;
        public AuthUserController(IAuthUserService authUserService)
        {
            _authUserService = authUserService; 
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest model, [FromQuery]ResetPasswordRequest request, [FromQuery]EmailRequest emailRequest, [FromQuery] bool forgotPass)
        {
            var rs = await _authUserService.Login(model,model.Email,request,emailRequest,forgotPass);
            return Ok(rs);
        }

        [HttpPost("registeration")]
        public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterationRequest model)
        {
            var rs = await _authUserService.Registeration(model);
            return Ok(rs);
        }
    }
}
