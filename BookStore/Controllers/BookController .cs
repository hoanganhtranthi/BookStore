using AutoMapper;
using BookStore.Service;
using BookStore.Service.DTO.Request;
using DataAcess.RequestModels;
using DataAcess.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTQ.Sdk.Core.CustomModel;
using NTQ.Sdk.Core.ViewModels;
using System.Data;
using System.Net;

namespace BookStore_API.Controllers
{
    [Route("api/BookAPI")]
    [ApiController]
    public class BookController : ControllerBase
    {
       private readonly IBookRepository _bookRepository;
        public BookController(IBookRepository bookRepository)
        {
           _bookRepository = bookRepository;
        }
        [HttpGet("paging")]
        public async Task<ActionResult<List<BookReponseModel>>> GetBooksPaging([FromQuery]BookRequestModel model, [FromQuery] PagingRequest? request)
        {
            var rs = await _bookRepository.GetBooks(request, model);
            return Ok(rs);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookReponseModel>> GetBookById(int id)
        {
            var rs = await _bookRepository.GetBook(id);
            return Ok(rs);
        }
        [HttpGet("category/{cateId:int}")]
        public async Task<ActionResult<BookReponseModel>> GetBookByCateId(int cateId)
        {
            var rs = await _bookRepository.GetBookByCateById(cateId);
            return Ok(rs);
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<BookReponseModel>> CreateBook([FromBody] BookRequestModel model)
        {
            var rs=await _bookRepository.CreateBook(model);
            return Ok();
        }
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<BookReponseModel>> DeleteBook(int id)
        {
            var rs = await _bookRepository.DeleteBook(id);
            return Ok();
        }
        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<BookReponseModel>>UpdateBook(int id, [FromBody] BookRequestModel model)
        {
            var rs=await _bookRepository.UpdateBook(id, model);
            return Ok(rs);
        }
    }
}
