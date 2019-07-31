using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RubberDucky.Common.Extensions;
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

                context.SetList(InitMessages());
                context.SetList(DataLoader.Get<Product>());
                context.SetList(DataLoader.Get<OrderDetail>());
                context.SetList(DataLoader.Get<Order>());
                context.SetList(DataLoader.Get<Customer>());

                stopwatch.Stop();
                Console.WriteLine($"All Data loaded in {stopwatch.ElapsedMilliseconds}ms");
                context.SaveChanges();
            }
        }

        private static List<Message> InitMessages()
        {
            return new List<Message>(){
                new Message()
                {
                    Id = Guid.NewGuid().ToString(),
                    IsUser = false,
                    Recieved = DateTime.Now,
                    Text = "Quack Quack"
                }
            };
        }
    }
}