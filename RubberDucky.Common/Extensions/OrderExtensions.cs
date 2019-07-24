using Microsoft.EntityFrameworkCore;
using RubberDucky.Common.Data;
using RubberDucky.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Common.Extensions
{
    public static class OrderExtensions
    {
        public static void AddConfirmedOrderDetail(this Order order, OrderDetail confirmedDetail)
        {
            if (order.ConfirmedOrderDetails.Any(x => x.ProductID == confirmedDetail.ProductID))
            {
                order.ConfirmedOrderDetails.First(x => x.ProductID == confirmedDetail.ProductID).Quantity += confirmedDetail.Quantity;
            }
            else
            {
                order.ConfirmedOrderDetails.Add(confirmedDetail);
            }
            order.StagedOrderDetails.Remove(confirmedDetail);
        }

        public static void AddStageOrderDetail(this Order order, OrderDetail stageDetail)
        {
            if (order.StagedOrderDetails.Any(x => x.ProductID == stageDetail.ProductID))
            {
                order.StagedOrderDetails.First(x => x.ProductID == stageDetail.ProductID).Quantity += stageDetail.Quantity;
            }
            else
            {
                order.StagedOrderDetails.Add(stageDetail);
            }
        }

        public static void AddStageOrderDetail(this Order order, int quantity, Product product)
        {
            var detail = new OrderDetail()
            {
                OrderDetailID = Guid.NewGuid().ToString(),
                ProductID = product.ProductID,
                UnitPrice = product.UnitPrice,
                Quantity = quantity
            };
            order.AddStageOrderDetail(detail);
        }
    }
}