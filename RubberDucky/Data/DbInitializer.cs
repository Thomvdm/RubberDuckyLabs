using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RubberDucky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Data
{
    /// <summary>
    /// Adds the loaded data from the source to the AppDbContext
    /// </summary>
    public static class DbInitializer
    {
        
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new AppDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
            {
                //Messages
                if (!context.Set<Message>().Any())
                {
                    context.Set<Message>().Add(new Message()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IsUser = false,
                        Recieved = DateTime.Now,
                        Text = "Quack Quack"
                    });

                }
                if (!context.Set<Product>().Any())
                {
                    var products = DataLoader.GetProducts();
                    foreach(var product in products)
                    {
                        context.Set<Product>().Add(product);
                    }
                }

                if (!context.Set<OrderDetail>().Any())
                {
                    var orderDetails = DataLoader.GetOrderDetails();
                    foreach (var detail in orderDetails)
                    {
                        context.Set<OrderDetail>().Add(detail);
                    }
                }

                if (!context.Set<Order>().Any())
                {
                    var orders = DataLoader.GetOrders();
                    foreach (var order in orders)
                    {
                        context.Set<Order>().Add(order);
                    }
                }

                if (!context.Set<Customer>().Any())
                {
                    var customers = DataLoader.GetCustomers();
                    foreach (var customer in customers)
                    {
                        context.Set<Customer>().Add(customer);
                    }
                }

                context.SaveChanges();
            }

        }
    }
}
