using Microsoft.Recognizers.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Business.Interface
{
    public interface IMessageProcessor
    {
        void SetOrderId(string orderId);
        void UpdateBasedOnRecievedNumbers(List<ModelResult> numbers, Dictionary<int, string> words);
        void UpdateBasedOnConfirmation(bool isConfirming, double confidence);
    }
}
