using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Business
{
    public class ResponseConfirmationEventArgs: EventArgs
    {
        public bool IsConfirming { get; set; }
        public double Confidence { get; set; }
    }
}
