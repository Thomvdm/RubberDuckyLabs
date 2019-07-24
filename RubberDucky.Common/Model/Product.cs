using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Common.Model
{
    public class Product
    {
        public string ProductName { get; set; }
        [Key]
        public string ProductID { get; set; }
        public double UnitPrice { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}
