using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BookStore.Service.DTO.Response
{
    public class SystemReportModel : SystemReportExcelModel
    {
        public int TotalOrder { get; set; }
        public double TotalAmount { get; set; }
        public double RefundAmount { get; set; }

        public double TotalFee { get; set; }

        public int TotalOrderNew { get; set; }
        public int TotalOrderReturned { get; set; }
        public int TotalOrderOverDue { get; set; }
        public int TotalOrderCancel { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
    public class SystemDayReportModel: DayReport<SystemReportModel>
    {

    }

    public class DayReport<T>
    {
        public DateTime Date { get; set; }
        public T Data { get; set; }
    }

    public class SystemReportExcelModel
    {
        public string DateReport { get; set; }
        public int TotalOrderExcel { get; set; }
        public List<SystemReportModel> ReportDetail { get; set; }

    }
}
