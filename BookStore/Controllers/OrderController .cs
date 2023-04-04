using AutoMapper;
using DataAcess.RequestModels;
using DataAcess.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTQ.Sdk.Core.CustomModel;
using NTQ.Sdk.Core.ViewModels;
using System.Data;
using System.Net;
using BookStore.Service.Service.InterfaceService;
using BookStore.Service.DTO.Request;
using BookStore.Service.Helpers;
using BookStore.Service.DTO.Response;
using BookStore.Service.Service.ImplService;

namespace OrderStore_API.Controllers
{
    [Route("api/OrderAPI")]
    [ApiController]
    public class OrderController : ControllerBase
    {
       private readonly IOrderRepository _orderRepository;
        private readonly IReportService _reportService;
        public OrderController(IOrderRepository orderRepository, IReportService reportService)
        {
            _orderRepository = orderRepository;
            _reportService = reportService;
        }
        [HttpGet("paging")]
        public async Task<ActionResult<List<OrderReponseModel>>> GetOrdersPaging([FromQuery]OrderRequestModel model, [FromQuery] PagingRequest request)
        {
            var rs = await _orderRepository.GetOrders(request, model);
            return Ok(rs);
        }
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderReponseModel>> GetOrderById(int id)
        {
            var rs = await _orderRepository.GetOrder(id);
            return Ok(rs);
        }
        [HttpGet("report")]
        public async Task<dynamic> GetOrdersReports()
        {
            var rs = await _orderRepository.GetOrdersReportByDate();
            return Ok(rs);
        }
        [HttpGet("reportInRange")]
        public async Task<ActionResult<SystemReportModel>> GetOrdersReports([FromQuery] DateFilterRequest request)
        {
            var rs = await _reportService.GetSystemDayReportInRange(request);
            return rs;
        }
        [HttpGet("reportByDay")]
        public async Task<ActionResult<List<SystemDayReportModel>>> GetOrdersReportInDay([FromQuery] DateTime dayReport)
        {
            var rs = await _reportService.GetSystemDayReportByDay(dayReport);
            return Ok(rs);
        }
        [HttpPost()]
        public async Task<ActionResult<List<OrderReponseModel>>> CreateOrder([FromBody] OrderCreateRequestModel model)
        {
            var rs=await _orderRepository.CreateOrder(model);
            return Ok(rs);
        }
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<OrderDetailReponseModel>> DeleteOrder(int id)
        {
            var rs = await _orderRepository.DeleteItemOfOrder(id);
            return Ok(rs);
        }
        [HttpPut("{id:int}")]
        public async Task<ActionResult<OrderDetailReponseModel>>UpdateOrder(int id, [FromBody] OrderDetailUpdateRequestModel model)
        {
            var rs=await _orderRepository.UpdateItemOfOrder(id, model);
            return Ok(rs);
        }
        [HttpPut()]
        public async Task<ActionResult<OrderReponseModel>> UpdateOrderStatus(int orderId, StatusType.StatusOrder orderStatus)
        { 
             var rs = await _orderRepository.UpdateStatusOrder(orderId, orderStatus);
             return Ok(rs);
        }

    }
}
