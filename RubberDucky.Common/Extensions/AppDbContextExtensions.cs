using Microsoft.EntityFrameworkCore;
using RubberDucky.Common.Data;
using RubberDucky.Common.Model;
using System;
using System.Threading.Tasks;

namespace RubberDucky.Common.Extensions
{
    public static class AppDbContextExtensions
    {
        public static async Task<Order> GetCurrentOrder(this AppDbContext dbContext, string orderId)
        {
            if(!(await dbContext.Set<Order>().AnyAsync(x => x.OrderID == orderId)))
            {
                await SetNewOrder(dbContext, orderId);
            }
            return await dbContext.Set<Order>()
                    .Include(o => o.ConfirmedOrderDetails)
                    .ThenInclude(od => od.Product)
                    .Include(o => o.StagedOrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(x => x.OrderID.Equals(orderId));
            
        }

        private static async Task SetNewOrder(AppDbContext dbContext, string orderId)
        {
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                var order = new Order()
                {
                    OrderID = orderId,
                    OrderDate = DateTime.Now
                };
                dbContext.Set<Order>().Add(order);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
