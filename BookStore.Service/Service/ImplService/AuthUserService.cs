using AutoMapper;
using BookStore.Data.Models;
using BookStore.Data.UnitOfWork;
using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using BookStore.Service.Exceptions;
using BookStore.Service.Service.InterfaceService;
using FireSharp.Response;
using Google.Apis.Auth;
using Hangfire;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NTQ.Sdk.Core.CustomModel;
using NTQ.Sdk.Core.Filters;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.Service.ImplService
{
    public class AuthUserService : IAuthUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private string? _secretKey;
        private IMapper _mapper;
        private IEmailService _emailService;

        public AuthUserService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _secretKey = configuration.GetValue<string>("ApiSetting:Secret");
            _emailService = emailService;
        }

        public bool IsUniqueUser(string Email)
        {
            var user = _unitOfWork.Repository<User>().Find(u => u.Email == Email);
            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<BaseResponseViewModel<LoginResponse>> Login(LoginRequest loginRequest, string emailLogin, ResetPasswordRequest request, EmailRequest emailRequest, bool forgotPass)
        {
            try
            {
                var user = _unitOfWork.Repository<User>().GetAll()
                       .FirstOrDefault(u => u.Email.Equals(emailLogin.Trim()));

                if (user == null) throw new CrudException(HttpStatusCode.BadRequest, "User Not Found", "");

                if (!emailRequest.EmailVerified.Equals("") && emailRequest.Token.Equals(""))
                {

                    var randomToken = GenerateRandomNo();

                    //send verification code
                    EmailRequest emailVerified = new EmailRequest();
                    emailVerified.EmailVerified = emailRequest.EmailVerified;
                    string subject = "[Book Store] Verification code";
                    string body = $"Verification code: {randomToken} \n" +
                        "Verification code is valid for 60 seconds";

                    _emailService.SendEmail(emailVerified, subject, body);

                    user.VerificationToken = randomToken;
                    user.VerifiedAt = DateTime.Now;

                    await _unitOfWork.Repository<User>().Update(user, user.UserId);
                    await _unitOfWork.CommitAsync();

                }
                else if (!emailRequest.Token.Equals("") && !emailRequest.EmailVerified.Equals(""))
                {
                    if (user.VerificationToken != emailRequest.Token || DateTime.Now > user.VerifiedAt.Value.AddMinutes(1)) throw new CrudException(HttpStatusCode.BadRequest, "Invalid token", "");

                    user.VerifiedAt = DateTime.Now;
                    user.VerificationToken = emailRequest.Token;

                    await _unitOfWork.Repository<User>().Update(user, user.UserId);
                    await _unitOfWork.CommitAsync();
                }

                else if (forgotPass)
                {

                    user.PasswordResetToken = CreateRandomToken();
                    user.ResetTokenExpires = DateTime.Now.AddDays(1);

                    await _unitOfWork.Repository<User>().Update(user, user.UserId);
                    await _unitOfWork.CommitAsync();

                }

                else if (request.ConfirmPassword != null)
                {
                    if (user.PasswordResetToken != request.PasswordResetToken.Trim() || user.ResetTokenExpires < DateTime.Now)
                        throw new CrudException(HttpStatusCode.BadRequest, "Invalid Information", "");

                    CreatPasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);


                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;

                    await _unitOfWork.Repository<User>().Update(user, user.UserId);
                    await _unitOfWork.CommitAsync();
                }

                else if (!loginRequest.Password.Equals(""))
                {

                    if (!VerifyPasswordHash(loginRequest.Password, user.PasswordHash, user.PasswordSalt))
                        throw new CrudException(HttpStatusCode.BadRequest, "Password is incorrect", "");

                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_secretKey);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                        }),
                        Expires = DateTime.UtcNow.AddDays(7),
                        SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);

                    return new BaseResponseViewModel<LoginResponse>()
                    {
                        Status = new StatusViewModel
                        {
                            ErrorCode = 0,
                            Message = "success",
                            Success = true
                        },
                        Data = new LoginResponse
                        {
                            Token = tokenHandler.WriteToken(token),
                            User = _mapper.Map<UserResponse>(user)
                        }
                    };
                }
                return new BaseResponseViewModel<LoginResponse>()
                {
                    Status = new StatusViewModel
                    {
                        ErrorCode = 0,
                        Message = "success",
                        Success = true
                    },
                    Data = new LoginResponse
                    {
                        Token = "",
                        User = _mapper.Map<UserResponse>(user)
                    }
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Progress Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<BaseResponseViewModel<UserResponse>> Registeration(RegisterationRequest registerationRequest)
        {
            try
            {
                var isUnique = IsUniqueUser(registerationRequest.Email);
                if (!isUnique)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "User has already register", "");
                }
                if (registerationRequest == null)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "User information is invalid", "");
                }

                CreatPasswordHash(registerationRequest.Password, out byte[] passwordHash, out byte[] passwordSalt);

                var user = _mapper.Map<User>(registerationRequest);
                user.Role = "customer";
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;


                await _unitOfWork.Repository<User>().CreateAsync(user);

                await _unitOfWork.CommitAsync();
                return new NTQ.Sdk.Core.CustomModel.BaseResponseViewModel<UserResponse>()
                {
                    Status = new StatusViewModel
                    {
                        ErrorCode = 0,
                        Message = "success",
                        Success = true
                    },
                    Data = _mapper.Map<UserResponse>(user)
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Register Error!!!", ex.InnerException?.Message);
            }
        }

        private void CreatPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }


        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }


        public string GenerateRandomNo()
        {
            int _min = 0000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max).ToString();
        }

        private string CreateRandomToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
    }
}
