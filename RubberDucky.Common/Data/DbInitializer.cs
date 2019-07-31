using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RubberDucky.Common.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Common.Data
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
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var roundSW = new Stopwatch();
                roundSW.Start();
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
                roundSW.Stop();
                Console.WriteLine($"Messages loaded in {roundSW.ElapsedMilliseconds}ms");
                roundSW.Restart();
                if (!context.Set<Product>().Any())
                {
                    var products = DataLoader.GetProducts();
                    foreach(var product in products)
                    {
                        context.Set<Product>().Add(product);
                    }
                }
                roundSW.Stop();
                Console.WriteLine($"Products loaded in {roundSW.ElapsedMilliseconds}ms");
                roundSW.Restart();
                if (!context.Set<OrderDetail>().Any())
                {
                    var orderDetails = DataLoader.GetOrderDetails();
                    foreach (var detail in orderDetails)
                    {
                        context.Set<OrderDetail>().Add(detail);
                    }
                }
                roundSW.Stop();
                Console.WriteLine($"OrderDetails loaded in {roundSW.ElapsedMilliseconds}ms");
                roundSW.Restart();
                if (!context.Set<Order>().Any())
                {
                    var orders = DataLoader.GetOrders();
                    foreach (var order in orders)
                    {
                        context.Set<Order>().Add(order);
                    }
                }
                roundSW.Stop();
                Console.WriteLine($"Orders loaded in {roundSW.ElapsedMilliseconds}ms");
                roundSW.Restart();
                if (!context.Set<Customer>().Any())
                {
                    var customers = DataLoader.GetCustomers();
                    foreach (var customer in customers)
                    {
                        context.Set<Customer>().Add(customer);
                    }
                }
                roundSW.Stop();
                Console.WriteLine($"Customers loaded in {roundSW.ElapsedMilliseconds}ms");

                stopwatch.Stop();
                Console.WriteLine($"All Data loaded in {stopwatch.ElapsedMilliseconds}ms");
                context.SaveChanges();
            }
        }
    }
}