using AutoMapper;
using BookStore.Data.Models;
using BookStore.Data.UnitOfWork;
using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using BookStore.Service.Exceptions;
using BookStore.Service.Helpers;
using BookStore.Service.Service.InterfaceService;
using DataAcess.ResponseModels;
using Microsoft.EntityFrameworkCore;
using NTQ.Sdk.Core.CustomModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.Service.ImplService
{
    public class ReturnOrderService : IReturnOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReturnOrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NTQ.Sdk.Core.CustomModel.BaseResponseViewModel<OrderReponseModel>> ReturnOrder(ReturnOrderRequest returnRequest, int userId)
        {
            try
            {
                //kiểm tra user có input chưa
                if (returnRequest == null)
                    throw new CrudException(HttpStatusCode.NotFound, "Information Invalid", "");


                //lấy user dựa theo userID được input
                var user = _unitOfWork.Repository<User>().GetAll().FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                    throw new CrudException(HttpStatusCode.NotFound, "User Not Found", "");

                //lấy orderBook đang được mượn của userID được nhập
                var orderBook = _unitOfWork.Repository<OrderBook>().GetAll().Include(u => u.OrderDetails)
                                                        .FirstOrDefault(u => u.OrderId == returnRequest.OrderID && u.UserId == userId);
                if(orderBook == null) 
                    throw new Exception();


                //ktra nếu status được trả rồi thì báo lỗi
                if (orderBook.Status == (int)StatusType.StatusOrder.Returned)
                    throw new CrudException(HttpStatusCode.NotFound, "Information Invalid", "");


                var orderDetailList = orderBook.OrderDetails.ToList();

                //lấy orderdetail dựa trên bookID user muốn trả 
                var orderDetail = orderDetailList.FirstOrDefault(u => u.BookId == returnRequest.BookID);
                if (orderDetail == null)
                    throw new CrudException(HttpStatusCode.NotFound, "BookId has no order detail of this user", "");

                //quantity trả mà lớn hơn quantity mượn => bảo lỗi
                if (returnRequest.Quantity > orderDetail.Quantity - orderDetail.ReturnedQuantity)
                    throw new CrudException(HttpStatusCode.NotFound, "The returned quantity is higher than borrowed quantity", "");


                if (returnRequest.Quantity > orderDetail.Quantity) throw new CrudException(HttpStatusCode.NotFound, "The returned quantity is higher than borrowed quantity", "");

                var book = _unitOfWork.Repository<Book>().GetAll()
                                      .FirstOrDefault(u => u.BookId == returnRequest.BookID);
                if (book == null)
                    throw new CrudException(HttpStatusCode.NotFound, "Book Not Found", "");

                //2 trường hợp:
                // TH1: trả hết 
                if (orderDetail.Quantity - returnRequest.Quantity - orderDetail.ReturnedQuantity == 0)
                {
                    orderDetail.ReturnedQuantity += returnRequest.Quantity;
                    book.CurrentQuantity += returnRequest.Quantity; // update lại số lượng sách có trong kho
                }

                // TH2: không trả đủ
                if (orderDetail.Quantity - returnRequest.Quantity - orderDetail.ReturnedQuantity > 0)
                {
                    orderDetail.ReturnedQuantity += returnRequest.Quantity;
                    book.CurrentQuantity += returnRequest.Quantity; // update lại số lượng sách có trong kho
                }

                //ktra các orderDetails đã được trả hết chưa 
                int check = 0;
                foreach (var item in orderDetailList)
                {
                    if (item.Quantity == item.ReturnedQuantity)
                    {
                        check++;
                    }
                };
                if (check == orderDetailList.Count)
                {
                    orderBook.Status = (int)StatusType.StatusOrder.Returned;
                    orderBook.OrderReturnDate = DateTime.Now;
                }
                else
                {
                    orderBook.Status = (int)StatusType.StatusOrder.Borrowing;
                }

                //
                var responseOrder = _mapper.Map<OrderReponseModel>(orderBook);
                var responesOrderDetail = _mapper.Map<OrderDetailReponseModel>(orderDetail);
                var responseBook = _mapper.Map<BookReponseModel>(orderDetail.Book);

                await _unitOfWork.Repository<OrderBook>().Update(orderBook, orderBook.OrderId);
                await _unitOfWork.CommitAsync();
                await _unitOfWork.Repository<OrderDetail>().Update(orderDetail, orderDetail.OrderDetailId);
                await _unitOfWork.CommitAsync();
                await _unitOfWork.Repository<Book>().Update(orderDetail.Book, orderDetail.Book.BookId);
                await _unitOfWork.CommitAsync();
                return new NTQ.Sdk.Core.CustomModel.BaseResponseViewModel<OrderReponseModel>()
                {
                    Status = new StatusViewModel
                    {
                        ErrorCode = 0,
                        Message = "success",
                        Success = true,
                    },
                    Data = responseOrder
                };
            }
            catch(Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Return Order Error!!!", ex.InnerException?.Message);
            }
        }
    }
}
