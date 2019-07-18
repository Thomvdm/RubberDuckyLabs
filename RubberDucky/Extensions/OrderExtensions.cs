using RubberDucky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Extensions
{
    public static class OrderExtensions
    {
        public static void AddConfirmedOrderDetail(this Order order, OrderDetail confirmedDetail)
        {
            order.ConfirmedOrderDetails.Add(confirmedDetail);
            order.StagedOrderDetails.Remove(confirmedDetail);
        }

        public static void AddStageOrderDetail(this Order order, OrderDetail stageDetail)
        {
            order.StagedOrderDetails.Add(stageDetail);
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