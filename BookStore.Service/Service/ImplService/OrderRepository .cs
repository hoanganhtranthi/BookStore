
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure;
using Azure.Core;
using BookStore.Data.Extensions;
using BookStore.Data.Models;
using BookStore.Data.UnitOfWork;
using BookStore.Service;
using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using BookStore.Service.Exceptions;
using BookStore.Service.Helpers;
using BookStore.Service.Service.InterfaceService;
using DataAcess.RequestModels;
using DataAcess.ResponseModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NTQ.Sdk.Core.Attributes;
using NTQ.Sdk.Core.BaseConnect;
using NTQ.Sdk.Core.CustomModel;
using NTQ.Sdk.Core.Utilities;
using NTQ.Sdk.Core.ViewModels;
using Org.BouncyCastle.Asn1.Ocsp;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;
        public OrderRepository(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<BaseResponseViewModel<List<OrderReponseModel>>> CreateOrder(OrderCreateRequestModel request)
        {
            try
            {
                List<OrderReponseModel> listOrderResult = new List<OrderReponseModel>();

                //validate xem order cua user co order nao dang qua han khong
                var order = _unitOfWork.Repository<OrderBook>().GetAll().Where(a => a.UserId == request.UserId
                && a.Status == (int)StatusType.StatusOrder.Borrowing && a.OrderReturnDate < DateTime.Now);

                if (!order.IsNullOrEmpty())
                    throw new CrudException(HttpStatusCode.BadRequest, "Borrowing Book Fail", "");

                OrderBook orderBook = new OrderBook();
                orderBook.OrderDate = DateTime.Now;
                orderBook.OrderReturnDate = DateTime.Now.AddMonths(2);
                orderBook.Status = (int)StatusType.StatusOrder.Borrowing;
                orderBook.User = _unitOfWork.Repository<User>().GetAll()
                .FirstOrDefault(x => x.UserId == request.UserId);
                orderBook.UserId = request.UserId;

                List<OrderDetail> listOrderDetail = new List<OrderDetail>();
                List<OrderDetailReponseModel> listOrderDetailResponse = new List<OrderDetailReponseModel>();
                orderBook.TotalPrice = 0;

                foreach (var detail in request.OrderDetails)
                {
                    //Add orderDetail vào listOderDetail
                    var product = _unitOfWork.Repository<Book>().GetAll()
                        .Include(x => x.Cate)
                        .Where(x => x.BookId == detail.BookId)
                        .FirstOrDefault();
                    orderBook.TotalPrice += (double)(product.Price * detail.Quantity);

                    //check quantity order có lớn hơn currentQuantity của Book
                    if (detail.Quantity > product.CurrentQuantity)
                        throw new CrudException(HttpStatusCode.BadRequest, "Information Invalid!", "");

                    OrderDetail orderDetail = new OrderDetail();
                    orderDetail.BookName = product.BookName;
                    orderDetail.Quantity = detail.Quantity;
                    orderDetail.Price = product.Price * detail.Quantity;
                    orderDetail.BookId = product.BookId;
                    orderDetail.ReturnedQuantity = 0;

                    listOrderDetail.Add(orderDetail);

                    // update lai currentQuantity của book 
                    product.CurrentQuantity = product.CurrentQuantity - detail.Quantity;
                    await _unitOfWork.Repository<Book>().Update(product, product.BookId);

                    //Add orderDetailResponse vào list OrderDetailResponse
                    var orderResult = _mapper.Map<OrderDetail, OrderDetailReponseModel>(orderDetail);
                    orderResult.BookImg = product.BookImg;
                    listOrderDetailResponse.Add(orderResult);
                }
                //gán listOrderDetail vào List<OrderDetail> của OrderBook
                orderBook.OrderDetails = listOrderDetail;

                //tạo orderBook và save changes
                await _unitOfWork.Repository<OrderBook>().CreateAsync(orderBook);
                await _unitOfWork.CommitAsync();

                //add order vào list kết quả để return
                var user = _mapper.Map<User, UserResponse>(orderBook.User);

                var result = _unitOfWork.Repository<OrderBook>().GetAll()
                    .Include(x => x.OrderDetails)
                    .Select(x => new OrderReponseModel()
                    {
                        OrderId = x.OrderId,
                        OrderDate = x.OrderDate,
                        OrderReturnDate = x.OrderReturnDate,
                        Status = x.Status,
                        User = user,
                        TotalPrice = x.TotalPrice,
                        OrderDetail = listOrderDetailResponse
                    }).FirstOrDefault();

                listOrderResult.Add(result);

                return new BaseResponseViewModel<List<OrderReponseModel>>()
                {
                    Status = new StatusViewModel()
                    {
                        Message = "Sucess",
                        Success = true,
                        ErrorCode = 0
                    },
                    Data = listOrderResult
                };
            }

            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Create Order Error !", ex.InnerException?.Message);
            }
        }

        public async Task<BaseResponseViewModel<OrderDetailReponseModel>> DeleteItemOfOrder(int id)
        {
            var rs = _unitOfWork.Repository<OrderDetail>().GetAll().FirstOrDefault(a => a.OrderDetailId == id);

            if (rs == null)
                throw new CrudException(HttpStatusCode.NotFound, "Order Detail ID Is Not Found", "");
            try
            {
                //xoa orderDetail 
                _unitOfWork.Repository<OrderDetail>().Delete(rs);

                //update lai currentQuantity cua Book
                var book = _unitOfWork.Repository<Book>().GetAll().FirstOrDefault(x => x.BookId == rs.BookId);

                book.CurrentQuantity = book.CurrentQuantity + rs.Quantity;

                await _unitOfWork.Repository<Book>().Update(book, book.BookId);

                //update lai totalPrice cua orderBook
                var orderBook = _unitOfWork.Repository<OrderBook>().GetAll().FirstOrDefault(x => x.OrderId == rs.OrderId);
                orderBook.TotalPrice -= rs.Price;

                await _unitOfWork.Repository<OrderBook>().Update(orderBook, orderBook.OrderId);

                await _unitOfWork.CommitAsync();

                var orderDetail = _mapper.Map<OrderDetailReponseModel>(rs);
                orderDetail.BookImg = book.BookImg;

                return new BaseResponseViewModel<OrderDetailReponseModel>()
                {
                    Status = new StatusViewModel()
                    {
                        Message = "Sucess",
                        Success = true,
                        ErrorCode = 0
                    },
                    Data = orderDetail
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Delete Order Detail Error !", ex.InnerException?.Message);
            }
        }

        public async Task<BaseResponseViewModel<OrderReponseModel>> GetOrder(int id)
        {
            try
            {
                var cacheData = _cacheService.GetData<OrderReponseModel>($"Order{id}");

                if (cacheData == null)
                {
                    var model = _unitOfWork.Repository<OrderBook>().GetAll().Where(a => a.OrderId == id)
                .Select(a => new OrderReponseModel
                {
                    OrderDate = a.OrderDate,
                    OrderId = a.OrderId,
                    OrderReturnDate = a.OrderReturnDate,
                    Status = a.Status,
                    TotalPrice = a.TotalPrice,
                    User = _mapper.Map<User, UserResponse>(a.User),
                    OrderDetail = new List<OrderDetailReponseModel>
                    (a.OrderDetails.Select(a => new OrderDetailReponseModel
                    {
                        BookId = a.BookId,
                        Price = a.Price,
                        Quantity = a.Quantity,
                        BookImg = _unitOfWork.Repository<Book>()
                        .GetAll().FirstOrDefault(a => a.BookId == a.BookId).BookImg,
                        BookName = a.BookName,

                    }))
                }).SingleOrDefault();

                    if (model == null)
                        throw new CrudException(HttpStatusCode.NotFound, "Order Id Is Not Found", "");

                    var expiryTime = DateTimeOffset.Now.AddMinutes(2);
                    _cacheService.SetData<OrderReponseModel>($"Order{id}", model, expiryTime);

                    return new BaseResponseViewModel<OrderReponseModel>()
                    {
                        Status = new StatusViewModel()
                        {
                            Message = "Sucess",
                            Success = true,
                            ErrorCode = 0
                        },
                        Data = model
                    };
                }

                return new BaseResponseViewModel<OrderReponseModel>()
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
                throw new CrudException(HttpStatusCode.BadRequest, "Get Order By Id Error !", ex.InnerException?.Message);
            }
        }
        public async Task<BasePagingViewModel<OrderReponseModel>> GetOrders(PagingRequest request, OrderRequestModel model)
        {
            try
            {
                var filter = _mapper.Map<OrderReponseModel>(model);
                filter.SortDirection = request.SortDirection;
                filter.SortProperty = request.SortProperty;

                var response = _unitOfWork.Repository<OrderBook>().GetAll()
                 .Select(a => new OrderReponseModel
                 {
                     OrderDate = a.OrderDate,
                     OrderId = a.OrderId,
                     OrderReturnDate = a.OrderReturnDate,
                     Status = a.Status,
                     TotalPrice = a.TotalPrice,
                     User = _mapper.Map<User, UserResponse>(a.User),
                     OrderDetail = new List<OrderDetailReponseModel>
                    (a.OrderDetails.Select(a => new OrderDetailReponseModel
                    {
                        BookId = a.BookId,
                        Price = a.Price,
                        Quantity = a.Quantity,
                        BookImg = _unitOfWork.Repository<Book>()
                        .GetAll().FirstOrDefault(a => a.BookId == a.BookId).BookImg,
                        ReturnedQuantity = a.ReturnedQuantity,
                        BookName = a.BookName,

                    }))
                 })
                .Where(a => a.OrderDate >= model.OrderDate && a.OrderReturnDate >= model.OrderReturnDate).DynamicFilter(filter).DynamicSort(filter)
                .PagingQueryable(request.Page, request.PageSize).Item2;

                return new BasePagingViewModel<OrderReponseModel>()
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


        public async Task<BaseResponseViewModel<OrderDetailReponseModel>> UpdateItemOfOrder(int id, OrderDetailUpdateRequestModel order)
        {
            var rs = _unitOfWork.Repository<OrderDetail>().GetAll().FirstOrDefault(a => a.OrderDetailId == id);

            if (rs == null)
                throw new CrudException(HttpStatusCode.NotFound, "", "");

            try
            {
                //update lai currentQuantity cua Book
                var book = _unitOfWork.Repository<Book>().GetAll().FirstOrDefault(x => x.BookId == rs.BookId);

                book.CurrentQuantity = book.CurrentQuantity - (order.Quantity - rs.Quantity);

                await _unitOfWork.Repository<Book>().Update(book, book.BookId);

                //update lai totalPrice cua order
                var orderBook = _unitOfWork.Repository<OrderBook>().GetAll().FirstOrDefault(x => x.OrderId == rs.OrderId);
                orderBook.TotalPrice = orderBook.TotalPrice - rs.Price + order.Quantity * book.Price;
                await _unitOfWork.Repository<OrderBook>().Update(orderBook, orderBook.OrderId);

                //update lai price cua orderDetail
                var updateOrder = _mapper.Map<OrderDetailUpdateRequestModel, OrderDetail>(order, rs);
                updateOrder.Price = book.Price * updateOrder.Quantity;
                await _unitOfWork.Repository<OrderDetail>().Update(updateOrder, id);

                //gan bookImg vao OrderDetailResponseModel
                var updateOrderDetail = _mapper.Map<OrderDetailReponseModel>(rs);
                updateOrderDetail.BookImg = book.BookImg;

                await _unitOfWork.CommitAsync();
                return new BaseResponseViewModel<OrderDetailReponseModel>()
                {
                    Status = new StatusViewModel()
                    {
                        Message = "Sucess",
                        Success = true,
                        ErrorCode = 0
                    },
                    Data = updateOrderDetail
                };
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Update Order Error !", ex.InnerException?.Message);
            }
        }

      public async  Task<BaseResponseViewModel<OrderReponseModel>> UpdateStatusOrder(int id, StatusType.StatusOrder orderStatus)
        {
            try
            {
                var order = await _unitOfWork.Repository<OrderBook>().GetAll()
                            .Where(x => x.OrderId == id)
                            .FirstOrDefaultAsync();
                order.Status = (int)orderStatus;

                //cancel order
                if (order.Status == 3)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        //update lai currentQuantity cua Book
                        var product = _unitOfWork.Repository<Book>().GetAll()
                            .Include(x => x.Cate)
                            .Where(x => x.BookId == detail.BookId)
                            .FirstOrDefault();
                        product.CurrentQuantity = product.CurrentQuantity + detail.Quantity;

                        await _unitOfWork.Repository<Book>().Update(product, product.BookId);
                    }                   

                }

                await _unitOfWork.Repository<OrderBook>().Update(order,order.OrderId);
                await _unitOfWork.CommitAsync();

                return new BaseResponseViewModel<OrderReponseModel>()
                {
                    Status = new StatusViewModel()
                    {
                        Message = "Sucess",
                        Success = true,
                        ErrorCode = 0
                    },
                    Data = _mapper.Map<OrderReponseModel>(order)
                };
            }
            catch (Exception e)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Update Status Order Error", e.Message);
            }
        }

        public async Task<dynamic> GetOrdersReportByDate()
        {
            var startdate = DateTime.Today;
            var endDate = startdate.AddDays(+1);
            var ordersTotal = _unitOfWork.Repository<OrderBook>()
                .FindAll(p =>
                    p.OrderDate >= startdate && p.OrderDate <= endDate)
                .AsNoTracking().Count();
            var ordersNew = _unitOfWork.Repository<OrderBook>()
                .FindAll(p =>
                     p.OrderDate >= startdate && p.OrderDate <= endDate && p.Status ==(int) StatusType.StatusOrder.Borrowing)
                .AsNoTracking().Count();
            var ordersCancel = _unitOfWork.Repository<OrderBook>()
                .FindAll(p =>
                    p.OrderDate >= startdate && p.OrderDate <= endDate && p.Status == (int)StatusType.StatusOrder.Cancel)
                .AsNoTracking().Count();
            return new
            {
                Date = startdate,
                Total = ordersTotal,
                New = ordersNew,
                Cancel = ordersCancel
            };
        }
    }
}
