using Microsoft.Recognizers.Text;
using RubberDucky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Business.Interface
{
    public interface IDialogResponseBuilder
    {
        Task Acknowledgement(bool isConfirming, double confidence);
        Task ImplicitConfirmation(List<ModelResult> numbers, Dictionary<int, string> words);
        Task Prompt();
        Task FillPrompt();
        Task FinalizePrompt();
        void DefaultResponse();
        Task StoreSendMessage(Message inputMessage);
        Task StoreRecievedMessage(Message message);
    }
}
