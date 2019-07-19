using Microsoft.EntityFrameworkCore;
using RubberDucky.Data;
using RubberDucky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Extensions
{
    public static class AppDbContextExtensions
    {
        public static async Task<Order> GetCurrentOrder(this AppDbContext dbContext, string orderId)
        {
            return await dbContext.Set<Order>()
                    .Include(o => o.ConfirmedOrderDetails)
                    .ThenInclude(od => od.Product)
                    .Include(o => o.StagedOrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(x => x.OrderID.Equals(orderId));
            
        }
    }
}
