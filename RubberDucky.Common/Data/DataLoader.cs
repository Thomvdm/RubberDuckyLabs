using RubberDucky.Common.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RubberDucky.Common.Data
{
    /// <summary>
    /// Dataloader class loads data from the northwind.csv and maps them to the domain objects.
    /// </summary>
    public static class DataLoader
    {
        private static string _fileLocation;

        public static string NorthWindLocation
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_fileLocation))
                {
                    _fileLocation = Directory.GetCurrentDirectory();
                    if (!_fileLocation.Contains("bin"))
                    {
                        _fileLocation += "/bin/Debug/netcoreapp2.1/Data/northwind.csv";
                    }
                    else
                    {
                        _fileLocation += "/Data/northwind.csv";
                    }
                }
                return _fileLocation;
            }
        }

        public static List<T> Get<T>() where T : class
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = new List<T>();
            if(typeof(T) == typeof(Customer))
            {
                result = GetCustomers().Cast<T>().ToList();
            }
            else if (typeof(T) == typeof(OrderDetail))
            {
                result = GetOrderDetails().Cast<T>().ToList();
            }
            else if (typeof(T) == typeof(Order))
            {
                result = GetOrders().Cast<T>().ToList();
            }
            else if (typeof(T) == typeof(Product))
            {
                result = GetProducts().Cast<T>().ToList();
            }
            stopwatch.Stop();
            Debug.WriteLine($"{typeof(T).Name}s added to context in {stopwatch.ElapsedMilliseconds}ms");
            return result;
        }

        public static IEnumerable<Customer> GetCustomers()
        {
            try
            {
                var customers = System.IO.File.ReadAllLines(NorthWindLocation)
                                            .SkipWhile((line) => line.StartsWith("CUSTOMERS") == false)
                                            .Skip(1)
                                            .TakeWhile((line) => line.StartsWith("END CUSTOMERS") == false);
                return (from line in customers
                        let fields = line.Split(',')
                        let custID = fields[0].Trim()
                        select new Customer()
                        {
                            CustomerID = custID,
                            CustomerName = fields[1].Trim(),
                            Address = fields[2].Trim(),
                            City = fields[3].Trim(),
                            PostalCode = fields[4].Trim()
                        });
            }
            catch (Exception)
            {
                return new List<Customer>();
            }
            
            
        }

        //  "10248, VINET, 7/4/1996 12:00:00 AM, 7/16/1996 12:00:00 AM
        public static IEnumerable<Order> GetOrders()
        {
            // Assumes we copied the file correctly!
            var orders = System.IO.File.ReadAllLines(NorthWindLocation)
                                            .SkipWhile((line) => line.StartsWith("ORDERS") == false)
                                             .Skip(1)
                                            .TakeWhile((line) => line.StartsWith("END ORDERS") == false);
            var oders = new List<Order>();
            foreach(var line in orders)
            {
                var fields = line.Split(',');
                try
                {
                    oders.Add(new Order()
                    {
                        OrderID = fields[0],
                        CustomerID = fields[1].Trim(),
                        OrderDate = DateTime.Parse(fields[2]),
                        ShippedDate = DateTime.Parse(fields[3])
                    });
                }
                catch (Exception)
                {
                    oders.Add(new Order()
                    {
                        OrderID = fields[0],
                        CustomerID = fields[1].Trim(),
                        OrderDate = DateTime.Now,
                        ShippedDate = DateTime.Now
                    });
                }
            }
            return oders;
        }

        public static IEnumerable<Product> GetProducts()
        {
            // Assumes we copied the file correctly!
            var products = System.IO.File.ReadAllLines(NorthWindLocation)
                                            .SkipWhile((line) => line.StartsWith("PRODUCTS") == false)
                                             .Skip(1)
                                            .TakeWhile((line) => line.StartsWith("END PRODUCTS") == false);
            return from line in products
                   let fields = line.Split(',')
                   select new Product()
                   {
                       ProductID = fields[0],
                       ProductName = fields[1].Trim(),
                       UnitPrice = Convert.ToDouble(fields[2])

                   };
        }

        public static IEnumerable<OrderDetail> GetOrderDetails()
        {
            // Assumes we copied the file correctly!
            var orderDetails = System.IO.File.ReadAllLines(NorthWindLocation)
                                            .SkipWhile((line) => line.StartsWith("ORDER DETAILS") == false)
                                             .Skip(1)
                                            .TakeWhile((line) => line.StartsWith("END ORDER DETAILS") == false);

            return from line in orderDetails
                   let fields = line.Split(',')
                   select new OrderDetail()
                   {
                       ConfirmedID = fields[0],
                       ProductID = fields[1],
                       UnitPrice = Convert.ToDouble(fields[2]),
                       Quantity = Convert.ToDouble(fields[3]),
                       Discount = Convert.ToDouble(fields[4])
                   };
        }
    }
}
