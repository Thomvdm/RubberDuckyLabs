using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Recognizers.Text;

namespace RubberDucky.Business
{
    public class ResponseNumberEventArgs : EventArgs
    {
        public IList<ModelResult> numbers { get; set; }
        public Dictionary<int, string> OriginalWords { get; set; }
    }
}
