
using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using DataAcess.RequestModels;
using DataAcess.ResponseModels;
using NTQ.Sdk.Core.CustomModel;
using NTQ.Sdk.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service
{
    public interface IBookRepository
    {
         Task<BaseResponseViewModel<BookReponseModel>> CreateBook(BookRequestModel model);
        Task<BaseResponseViewModel<BookReponseModel>> GetBook(int id);
        Task<BaseResponseViewModel<List<BookReponseModel>>> GetBookByCateById(int cateId);
        Task<BasePagingViewModel<BookReponseModel>> GetBooks(PagingRequest? request,BookRequestModel? model);
        Task<BaseResponseViewModel<BookReponseModel>>  UpdateBook(int id, BookRequestModel book);
        Task<BaseResponseViewModel<BookReponseModel>> DeleteBook(int id);

    }
}
