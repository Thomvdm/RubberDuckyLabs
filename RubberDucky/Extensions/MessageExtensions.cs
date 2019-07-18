using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;
using Microsoft.Recognizers.Text.Number;
using RubberDucky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Business
{
    public static class MessageExtensions
    {
        public static void CheckConformation(this Message message, out bool isConfirming, out double confidence)
        {
            isConfirming = false;
            confidence = 0.1;
            var recognizer = new ChoiceRecognizer(Culture.Dutch);
            var model = recognizer.GetBooleanModel();
            var check = model.Parse(message.Text);
            if (check.Count > 0)
            {
                double.TryParse(check.First().Resolution["score"].ToString(), out confidence);
                bool.TryParse(check.First().Resolution["value"].ToString(), out isConfirming);
                isConfirming = isConfirming && confidence > 0.5;
            }
        }

        public static void CheckOnNumber(this Message message, out List<ModelResult> modelResult)
        {
            var recognizer = new NumberRecognizer(Culture.Dutch);
            var model = recognizer.GetNumberModel();
            modelResult = model.Parse(message.Text);
        }
    }
}
