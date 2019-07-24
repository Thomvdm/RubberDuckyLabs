using RubberDucky.Common.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Common.Model
{
    public class Order
    {
        [Key]
        public string OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ShippedDate { get; set; }

        public string CustomerID { get; set; }
        public Customer Customer { get; set; }

        [InverseProperty("Confirmed")]
        public List<OrderDetail> ConfirmedOrderDetails { get; set; }

        [InverseProperty("Staged")]
        public List<OrderDetail> StagedOrderDetails { get; set; }
    }
}
