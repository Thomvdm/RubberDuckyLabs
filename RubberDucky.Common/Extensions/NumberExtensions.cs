using Microsoft.Recognizers.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Common.Extensions
{
    public static class NumberExtensions
    {
        public static int GetQuantity(this ModelResult number)
        {
            int quantity;
            int i;
            int resolution;
            int.TryParse(number.Text, out i);
            int.TryParse(number.Resolution.Values.First().ToString(), out resolution);
            quantity = i;
            if (i != resolution)
            {
                quantity = resolution;
            }
            return quantity;
        }
    }
}
