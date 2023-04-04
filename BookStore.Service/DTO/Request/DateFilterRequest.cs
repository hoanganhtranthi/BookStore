using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.DTO.Request
{
    public class DateFilterRequest
    {
        [DataType(DataType.DateTime)]
        public DateTime? FromDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ToDate { get; set; }
    }
}
