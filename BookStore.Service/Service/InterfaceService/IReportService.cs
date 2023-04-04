using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.Service.InterfaceService
{
    public interface IReportService
    {
        public Task<SystemReportModel> GetSystemDayReportInRange(DateFilterRequest filter);
        public Task<List<SystemDayReportModel>> GetSystemDayReportByDay(DateTime filter);

    }
}
