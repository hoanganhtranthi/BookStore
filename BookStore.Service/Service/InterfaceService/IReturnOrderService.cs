﻿using BookStore.Service.DTO.Request;
using BookStore.Service.DTO.Response;
using NTQ.Sdk.Core.CustomModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Service.Service.InterfaceService
{
    public interface IReturnOrderService
    {
        Task<BaseResponseViewModel<OrderResponse>> ReturnOrder(ReturnOrderRequest returnRequest, int userId);
    }
}
