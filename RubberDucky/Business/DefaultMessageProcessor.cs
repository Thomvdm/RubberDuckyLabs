using Microsoft.Recognizers.Text;
using RubberDucky.Business.Interface;
using RubberDucky.Data;
using RubberDucky.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Business
{
    public class DefaultMessageProcessor: IMessageProcessor
    {
        private readonly AppDbContext _db;
        private readonly IEntityRecognizer _entityRecognizer;
        private string _orderId;

        public DefaultMessageProcessor(AppDbContext db, IEntityRecognizer entityRecognizer)
        {
            _db = db;
            _entityRecognizer = entityRecognizer;
        }

        public async void UpdateBasedOnRecievedNumbers(List<ModelResult> numbers, Dictionary<int, string> words)
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

        public async void UpdateBasedOnConfirmation(bool isConfirming, double confidence)
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

        public void SetOrderId(string orderId)
        {
            _orderId = orderId;
        }
    }
}
