﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAcess.RequestModels
{
    public class BookRequestModel
    {
        public string? BookName { get; set; }
        public string? BookImg { get; set; }
        public string? BookDetail { get; set; }
        public int? CurrentQuantity { get; set; } = null;
        [ForeignKey("Category")]
        public int? CateID { get; set; }=null;
        public double? Price { get; set; }=null;
    }
}
