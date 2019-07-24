using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Common.Model
{
    public class OrderDetail
    {
        [Key]
        public string OrderDetailID { get; set; }
        public double UnitPrice { get; set; }
        public double Quantity { get; set; }
        public double Discount { get; set; }

        public string ConfirmedID { get; set; }
        public Order Confirmed { get; set; }

        public string StagedID { get; set; }
        public Order Staged { get; set; }

        public string ProductID { get; set; }
        public Product Product { get; set; }
    }
}
