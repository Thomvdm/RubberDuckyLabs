using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RubberDucky.Common.Model
{
    public class Customer
    {
        [Key]
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        public List<Order> Orders { get; set; }
    }
}
