using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;
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
        public event EventHandler RespondToConfirmation;
        public void CheckOnConfirmation(Message message)
        {
            var recognizer = new ChoiceRecognizer(Culture.Dutch);
            var model = recognizer.GetBooleanModel();
            var check = model.Parse(message.Text);
            var eventArgs = new ResponseConfirmationEventArgs
            {
                IsConfirming = false
            };
            if(check.Count > 0)
            {
                double confidence;
                double.TryParse(check.First().Resolution["score"].ToString(), out confidence);
                bool isConfirming;
                bool.TryParse(check.First().Resolution["value"].ToString(), out isConfirming);
                eventArgs = new ResponseConfirmationEventArgs
                {
                    IsConfirming = isConfirming && confidence > 0.5,
                    Confidence = confidence
                };
            }
            
            OnRespondToConfirmation(eventArgs);
        }
        protected virtual void OnRespondToConfirmation(ResponseConfirmationEventArgs e)
        {
            EventHandler handler = RespondToConfirmation;
            handler?.Invoke(this, e);
        }

    }
}
