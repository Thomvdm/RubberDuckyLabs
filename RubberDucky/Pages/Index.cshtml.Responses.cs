using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;
using RubberDucky.Business;
using RubberDucky.Data;
using RubberDucky.Extensions;
using RubberDucky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubberDucky.Pages
{
    public partial class IndexModel : PageModel
    {
        private async void UpdateBasedOnRecievedNumbers(List<ModelResult> numbers, Dictionary<int, string> words)
        {
            var order = await GetCurrentOrder();
            foreach (var number in numbers)
            {
                var likelyProducts = _entityRecognizer.GetProducts(words, number);
                
                if (likelyProducts.Count > 0)
                {
                    order.AddStageOrderDetail(GetQuantity(number), likelyProducts.FirstOrDefault());
                }
            }
            await _db.SaveChangesAsync();
        }

        private int GetQuantity(ModelResult number)
        {
            int quantity;
            int i;
            int resolution;
            int.TryParse(number.Text, out i);
            int.TryParse(number.Resolution.Values.First().ToString(), out resolution);
            quantity = i;
            if (i != resolution)
            {
                quantity = resolution;
            }
            return quantity;
        }

        private async void UpdateBasedOnConfirmation(bool isConfirming, double confidence)
        {
            var order = await GetCurrentOrder();
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

        private async void DefaultResponse()
        {
            var message = new Message();
            message.UpdateText("Ik kan alleen bestellingen afnemen. Hiervoor heb ik aantallen nodig. Wat wil je?");
            await StoreRecievedMessage(message);
        }

        private async Task ImplicitConfirmation(List<ModelResult> numbers, Dictionary<int, string> words)
        {
            var message = new Message();
            foreach (var number in numbers)
            {
                var likelyProduct = _entityRecognizer.GetProducts(words, number).FirstOrDefault();
                if (likelyProduct == null)
                {
                    message.Text = $"{number.Resolution.Values.First()} van wat?";
                    return;
                }
                else
                {
                    message.Text = $"{message.Text} {GetQuantity(number)} {likelyProduct.ProductName}";
                }


                if (numbers.IndexOf(number) == numbers.Count - 1)
                {
                    message.Text = $"{message.Text} als ik het goed begrijp?";
                }
                else if (numbers.Count > 1 && numbers.IndexOf(number) == numbers.Count - 2)
                {
                    message.Text = $"{message.Text} en";
                }
                else
                {
                    message.Text = $"{message.Text}, ";
                }
                await StoreRecievedMessage(message);
            }
            
        }

        private async Task Acknowledgement(bool isConfirming, double confidence)
        {
            var message = new Message();
            if (isConfirming)
            {
                message.UpdateText("Komt eraan!");
            }
            else if (confidence > 0.9)
            {
                message.UpdateText("Wat moet het zijn?");
            }
            await StoreRecievedMessage(message);
        }
    }
}
