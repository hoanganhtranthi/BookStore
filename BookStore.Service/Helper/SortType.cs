﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.Helper
{
    public static class SortType
    {
        public enum SortOrder
        {
            Ascending = 0,
            Descending = 1,
            None = 2,
        }
    }
}
