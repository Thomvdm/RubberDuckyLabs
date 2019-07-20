using Microsoft.Recognizers.Text;
using RubberDucky.Business.Interface;
using RubberDucky.Data;
using RubberDucky.Extensions;
using RubberDucky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Business
{
    public class OrderDialog: IDialog
    {
        private readonly AppDbContext _db;
        private readonly IEntityRecognizer _entityRecognizer;
        private readonly IDialogResponseBuilder _orderDialogResponseBuilder;
        private string _orderId;

        public OrderDialog(AppDbContext db, IEntityRecognizer entityRecognizer, IDialogResponseBuilder orderDialogResponseBuilder)
        {
            _db = db;
            _entityRecognizer = entityRecognizer;
            _orderDialogResponseBuilder = orderDialogResponseBuilder;
        }

        public async Task ProcessMessage(Message inputMessage, string orderId)
        {
            _orderId = orderId;
            var order = await _db.GetCurrentOrder(_orderId);
            List<ModelResult> numberResults;
            bool isConfirming;
            double confirmingConfidence;
            //To multithreading? Or Task?
            inputMessage.CheckOnNumber(out numberResults);
            inputMessage.CheckConformation(out isConfirming, out confirmingConfidence);
            var processedStaged = false;
            // If the confidence is high enough it could be that the user is confirming.
            if (confirmingConfidence > 0.5 && order.StagedOrderDetails.Count > 0)
            {
                UpdateBasedOnConfirmation(isConfirming, confirmingConfidence);
                await _orderDialogResponseBuilder.Acknowledgement(isConfirming, confirmingConfidence);
                processedStaged = true;
            }
            // Determine which response to use. If found numbers respond to the numbers.
            if (numberResults.Count > 0)
            {
                UpdateBasedOnRecievedNumbers(numberResults, inputMessage.Words);
                await _orderDialogResponseBuilder.ImplicitConfirmation(numberResults, inputMessage.Words);
            }
            // Prompt for something else
            if (order.StagedOrderDetails.Count == 0 && processedStaged)
            {
                _orderDialogResponseBuilder.Prompt();
            }
            else if (!isConfirming && confirmingConfidence > 0.9 && !processedStaged)
            {
                _orderDialogResponseBuilder.FinalizePrompt();
            }
            else if (isConfirming && !processedStaged)
            {
                _orderDialogResponseBuilder.FillPrompt();
            }

            // If there are none numbers found and user isn't confirming default resposne.
            if (numberResults.Count == 0 && confirmingConfidence < 0.5)
            {
                _orderDialogResponseBuilder.DefaultResponse();
            }
        }

        private async void UpdateBasedOnRecievedNumbers(List<ModelResult> numbers, Dictionary<int, string> words)
        {
            var order = await _db.GetCurrentOrder(_orderId);
            foreach (var number in numbers)
            {
                var likelyProducts = _entityRecognizer.GetProducts(words, number);

                if (likelyProducts.Count > 0)
                {
                    order.AddStageOrderDetail(number.GetQuantity(), likelyProducts.FirstOrDefault());
                }
            }
            await _db.SaveChangesAsync();
        }

        private async void UpdateBasedOnConfirmation(bool isConfirming, double confidence)
        {
            var order = await _db.GetCurrentOrder(_orderId);
            // if is confirming add staged orderdetails to the confirmed orderdetails
            if (isConfirming)
            {
                var amountStaged = order.StagedOrderDetails.Count;
                for (var i = 0; i < amountStaged; i++)
                {
                    order.AddConfirmedOrderDetail(order.StagedOrderDetails.First());
                }
            }
            // if opposing clear all staged order details
            else if (confidence > 0.9)
            {
                order.StagedOrderDetails.Clear();
            }
            await _db.SaveChangesAsync();
        }
    }
}
