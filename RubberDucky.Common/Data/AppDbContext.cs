using Microsoft.EntityFrameworkCore;
using RubberDucky.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Common.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasOne(p => p.Customer)
                .WithMany(b => b.Orders);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(p => p.Staged)
                .WithMany(b => b.StagedOrderDetails);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(p => p.Confirmed)
                .WithMany(b => b.ConfirmedOrderDetails);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(p => p.Product)
                .WithMany(b => b.OrderDetails);
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}
