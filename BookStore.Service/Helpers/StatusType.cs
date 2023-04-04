using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.Helpers
{
    public  static class StatusType
    {
        public enum StatusOrder
        {
            Returned = 0,
            Borrowing = 1,
            Overdue = 2,
            Cancel= 3
        }
    }
}
