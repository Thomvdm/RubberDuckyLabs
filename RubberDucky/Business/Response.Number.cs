using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using RubberDucky.Data;
using RubberDucky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Business
{
    public partial class Response
    {
        public event EventHandler RespondToNumber;
        public void CheckOnNumber(Message message)
        {
            var recognizer = new NumberRecognizer(Culture.Dutch);
            var model = recognizer.GetNumberModel();
            var check = model.Parse(message.Text);
            if (check.Count > 0)
            {
                var eventArgs = new ResponseNumberEventArgs()
                {
                    numbers = check,
                    OriginalWords = message.Words
                };
                OnRespondToNumber(eventArgs);
            }
        }
        protected virtual void OnRespondToNumber(ResponseNumberEventArgs e)
        {
            EventHandler handler = RespondToNumber;
            handler?.Invoke(this, e);
        }

    }
}
