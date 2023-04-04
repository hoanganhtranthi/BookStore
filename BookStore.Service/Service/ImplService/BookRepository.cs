
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Core;
using BookStore.Data.Extensions;
using BookStore.Data.Models;
using BookStore.Data.UnitOfWork;
using BookStore.Service;
using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using BookStore.Service.Exceptions;
using BookStore.Service.Helpers;
using DataAcess.RequestModels;
using DataAcess.ResponseModels;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NTQ.Sdk.Core.BaseConnect;
using NTQ.Sdk.Core.CustomModel;
using NTQ.Sdk.Core.Utilities;
using NTQ.Sdk.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BookStore.Service
{
    public class BookRepository : IBookRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public BookRepository(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cache;
        }

        public async Task<BaseResponseViewModel<BookReponseModel>> CreateBook(BookRequestModel model)
        {
            try
            {
                if (model == null) throw new CrudException(HttpStatusCode.BadRequest, "Input Invalid", "");

                var rs = _unitOfWork.Repository<Book>()
                    .GetAll()
                    .FirstOrDefault(a => a.BookName.ToLower() == model.BookName.ToLower());

                if (rs != null)
                    throw new CrudException(HttpStatusCode.BadRequest, "Book already exists!", "");

                var creatBook = _mapper.Map<BookRequestModel, Book>(model);

                await _unitOfWork.Repository<Book>().CreateAsync(creatBook);

                await _unitOfWork.CommitAsync();

                return new NTQ.Sdk.Core.CustomModel.BaseResponseViewModel<BookReponseModel>()
                {
                    Status = new StatusViewModel()
                    {
                        Message = "Sucess",
                        Success = true,
                        ErrorCode = 0
                    },
                    Data = _mapper.Map<BookReponseModel>(creatBook)
                };
            }

            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Create Book Error !", ex.InnerException?.Message);
            }
        }

        public async Task<BaseResponseViewModel<BookReponseModel>> DeleteBook(int id)
        {
            var rs = _unitOfWork.Repository<Book>().GetAll().FirstOrDefault(a => a.BookId == id);
            if (rs == null)
                throw new CrudException(HttpStatusCode.NotFound, "", "");
            try
            {
                _unitOfWork.Repository<Book>().Delete(rs);

                await _unitOfWork.CommitAsync();

                return new BaseResponseViewModel<BookReponseModel>()
                {
                    Status = new StatusViewModel()
                    {
                        Message = "Sucess",
                        Success = true,
                        ErrorCode = 0
                    },
                    Data = _mapper.Map<BookReponseModel>(rs)
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Delete Book Error !", ex.InnerException?.Message);
            }
        }


        public async Task<BaseResponseViewModel<BookReponseModel>> GetBook(int id)
        {
            try
            {
                var cacheData = _cacheService.GetData<BookReponseModel>($"Book{id}");
                if (cacheData == null)
                {

                    var model = _unitOfWork.Repository<Book>()
                    .GetAll().Include(c => c.Cate)
                    .FirstOrDefault(c => c.BookId == id);

                    if (model == null)
                        throw new CrudException(HttpStatusCode.NotFound, "", "");

                    var expiryTime = DateTimeOffset.Now.AddMinutes(2);
                    cacheData = _mapper.Map<BookReponseModel>(model);
                    _cacheService.SetData<BookReponseModel>($"Book{id}", cacheData, expiryTime);

                    return new BaseResponseViewModel<BookReponseModel>()
                    {
                        Status = new StatusViewModel()
                        {
                            Message = "Sucess",
                            Success = true,
                            ErrorCode = 0
                        },
                        Data = cacheData
                    };
                }

                return new BaseResponseViewModel<BookReponseModel>()
                {
                    Status = new StatusViewModel()
                    {
                        Message = "Sucess",
                        Success = true,
                        ErrorCode = 0
                    },
                    Data = cacheData
                };

            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Get Book By Id Error !", ex.InnerException?.Message);
            }
        }

        public async Task<BaseResponseViewModel<List<BookReponseModel>>> GetBookByCateById(int cateId)
        {
            try
            {
                var model = _unitOfWork.Repository<Book>()
                .GetAll()
                .Include(c => c.Cate)
                .Where(c => c.CateId == cateId).ToList();

                if (model == null)
                    throw new CrudException(HttpStatusCode.NotFound, "Get Book By Cate Id Not Found", "");

                return new BaseResponseViewModel<List<BookReponseModel>>()
                {
                    Status = new StatusViewModel()
                    {
                        Message = "Sucess",
                        Success = true,
                        ErrorCode = 0
                    },
                    Data = _mapper.Map<List<BookReponseModel>>(model)
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Get Book By Category Id Error !", ex.InnerException?.Message);
            }
        }
        public async Task<BasePagingViewModel<BookReponseModel>> GetBooks(PagingRequest? request, BookRequestModel model)
        {
            try
            {
                var filter = _mapper.Map<BookReponseModel>(model);

                filter.SortDirection = request.SortDirection;

                filter.SortProperty = request.SortProperty;

                var response = _unitOfWork
                    .Repository<Book>()
                    .GetAll()
                    .Include(a => a.Cate)
                    .ProjectTo<BookReponseModel>(_mapper.ConfigurationProvider)
                    .DynamicFilter(filter).DynamicSort(filter)
                    .PagingQueryable(request.Page, request.PageSize).Item2;
                return new BasePagingViewModel<BookReponseModel>()
                {
                    Data = response.ToList(),
                    Metadata = request

                };
            }

            catch (Exception e)
            {

                throw new CrudException(HttpStatusCode.BadRequest, "Get Books Paging Error!!!", e.InnerException?.Message);
            }
        }
        public async Task<BaseResponseViewModel<BookReponseModel>> UpdateBook(int id, BookRequestModel model)
        {


            var book = _unitOfWork.Repository<Book>()
                .GetAll()
                .Include(a => a.Cate)
                .FirstOrDefault(a => a.BookId == id);

            if (book == null)
                throw new CrudException(HttpStatusCode.NotFound, "Book Not Found", "");

            try
            {
                var updateBook = _mapper.Map<BookRequestModel, Book>(model, book);

                await _unitOfWork.Repository<Book>().Update(updateBook, id);

                await _unitOfWork.CommitAsync();

                return new BaseResponseViewModel<BookReponseModel>()
                {
                    Status = new StatusViewModel()
                    {
                        Message = "Sucess",
                        Success = true,
                        ErrorCode = 0
                    },
                    Data = _mapper.Map<BookReponseModel>(book)
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Update Book Error !", ex.InnerException?.Message);
            }
        }
    }
}
