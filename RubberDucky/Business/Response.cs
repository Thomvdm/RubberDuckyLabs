using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Business
{
    public partial class Response
    {
        public event EventHandler DefaultResponse;

        protected virtual void OnDefaultResponse(EventArgs e)
        {
            EventHandler handler = DefaultResponse;
            handler?.Invoke(this, e);
        }
    }
}
