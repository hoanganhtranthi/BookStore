using BookStore.Service.DTO.Request;
using BookStore.Service.Service.InterfaceService;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit.Text;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using System.Net;
using ServiceStack;
using System.Net.Mail;
using BookStore.Data.UnitOfWork;
using AutoMapper;
using NTQ.Sdk.Core.CustomModel;
using BookStore.Service.DTO.Response;
using BookStore.Service.Exceptions;
using System.Linq.Dynamic.Core.Tokenizer;
using BookStore.Data.Models;
using Hangfire;

namespace BookStore.Service.Service.ImplService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public EmailService(IConfiguration config, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _config = config;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public string GenerateRandomNo()
        {
            int _min = 0000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max).ToString();
        }
        public void SendEmail(EmailRequest request, string subject, string body)
        {
            try
            {
                MailMessage msg = new MailMessage();

                //Add your email address to the recipients
                msg.To.Add(request.EmailVerified);
                //Configure the address we are sending the mail from
                MailAddress address = new MailAddress(_config.GetSection("EmailUserName").Value);
                msg.From = address;
                msg.Subject = subject;
                msg.Body = body;

                //Configure an SmtpClient to send the mail.
                SmtpClient client = new SmtpClient();
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;
                client.Host = "smtp.gmail.com";
                client.Port = 587;


                //Setup credentials to login to our sender email address ("UserName", "Password")
                NetworkCredential credentials = new NetworkCredential(_config.GetSection("EmailUserName").Value, _config.GetSection("EmailPassword").Value);
                client.UseDefaultCredentials = false;
                client.Credentials = credentials;

                //Send the msg
                client.Send(msg);
            }

            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Send Email Error!!!", ex.InnerException?.Message);
            }
        }

    }
}