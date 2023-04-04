using AutoMapper;
using BookStore.Data.Models;
using BookStore.Data.UnitOfWork;
using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using BookStore.Service.Exceptions;
using BookStore.Service.Helpers;
using BookStore.Service.Service.InterfaceService;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.Service.ImplService
{
    public class ReportService:IReportService
    {
        private readonly IUnitOfWork  _unitOfWork;
        private IMapper _mapper;
        public ReportService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<SystemDayReportModel>> GetSystemDayReportByDay(DateTime filter)
        {
            if (filter == null)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Invalid day", "");
            }

            var from = filter.GetStartOfDate();
            var to = filter.GetEndOfDate();

            var listOrder = _unitOfWork.Repository<OrderBook>().GetAll().Where(x => x.OrderDate >= from && x.OrderDate <= to);

            var order=listOrder.GroupBy(x=>
            new
            {
                x.OrderDate.Date
            }).Select(x=> new SystemDayReportModel()
            {
                Date=(DateTime)x.Key.Date,
                Data=new SystemReportModel()
                {
                    TotalOrder=x.Count(),
                    TotalOrderNew=x.Where(x=>x.Status == (int)StatusType.StatusOrder.Borrowing).Count(),
                    TotalOrderOverDue=x.Where(x=>x.Status == (int)StatusType.StatusOrder.Overdue).Count(),
                    TotalOrderReturned=x.Where(x => x.Status == (int)StatusType.StatusOrder.Returned).Count(),
                    TotalOrderCancel = x.Where(x => x.Status == (int)StatusType.StatusOrder.Cancel).Count(),
                    TotalAmount =(double)x.Sum(o=>o.TotalPrice)
                 
                }
            });
            return order.ToList();

        }

        public  async Task<SystemReportModel> GetSystemDayReportInRange(DateFilterRequest filter)
        {
            var from = filter?.FromDate;
            var to = filter?.ToDate;

            if (from == null && to == null)
            {
                from = DateTimeUtils.GetLastAndFirstDateInCurrentMonth().Item1;
                to = DateTimeUtils.GetLastAndFirstDateInCurrentMonth().Item2;
            }

            from ??= DateTimeUtils.GetCurrentDate();
            to ??= DateTimeUtils.GetCurrentDate();

            if (DateTime.Compare((DateTime)from, (DateTime)to) > 0)
            {
                throw new CrudException(HttpStatusCode.BadRequest, "Invalid day", "");
            }

            from = ((DateTime)from).GetStartOfDate();
            to = ((DateTime)to).GetEndOfDate();

            var listOrder = _unitOfWork.Repository<OrderBook>().GetAll().Where(x => x.OrderDate >= from && x.OrderDate <= to).ToList();

            SystemReportModel report = new SystemReportModel()
            {
                TotalOrder = listOrder.Where(x => x.Status != (int)StatusType.StatusOrder.Overdue).Count(),
                TotalOrderNew = listOrder.Where(x => x.Status == (int)StatusType.StatusOrder.Borrowing).Count(),
                TotalOrderOverDue = listOrder.Where(x => x.Status == (int)StatusType.StatusOrder.Overdue).Count(),
                TotalOrderReturned = listOrder.Where(x => x.Status == (int)StatusType.StatusOrder.Returned).Count(),
                TotalOrderCancel = listOrder.Where(x => x.Status == (int)StatusType.StatusOrder.Cancel).Count(),
                TotalAmount = (double)listOrder.Where(x => x.Status == (int)StatusType.StatusOrder.Borrowing).Sum(o => o.TotalPrice),
                RefundAmount = (double)listOrder.Where(x =>
                                  x.Status != (int)StatusType.StatusOrder.Cancel).Sum(o => o.TotalPrice),
                FromDate = from,
                ToDate = to
            };
            return report;
        }
    }
}
