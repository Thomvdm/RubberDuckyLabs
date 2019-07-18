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
        private async void r_OnRespondToNumber(object sender, EventArgs e)
        {
            if (e is ResponseNumberEventArgs eventArgs)
            {
                var order = await GetCurrentOrder();
                var message = new Message();
                var quantity = 0;
                foreach (var number in eventArgs.numbers)
                {
                    var possibleProducts = _entityRecognizer.GetProducts(eventArgs.OriginalWords, number);
                    int i;
                    int resolution;
                    int.TryParse(number.Text, out i);
                    int.TryParse(number.Resolution.Values.First().ToString(), out resolution);
                    if(possibleProducts.Count > 0)
                    {
                        quantity = i;
                        if (i != resolution)
                        {
                            quantity = resolution;

                        }
                        order.AddStageOrderDetail(quantity, possibleProducts.FirstOrDefault());
                        
                    }
                    BuildResponse(message, number, quantity, possibleProducts.FirstOrDefault(), eventArgs);
                }
                await _db.SaveChangesAsync();
            }
        }

        private async void r_OnRespondToConfirmation(object sender, EventArgs e)
        {
            if(e is ResponseConfirmationEventArgs args)
            {
                var order = await GetCurrentOrder();
                var message = new Message();
                if (args.IsConfirming)
                {
                    message.UpdateText("Komt eraan!");
                    for(var i = 0; i< order.StagedOrderDetails.Count; i++)
                    {
                        order.AddConfirmedOrderDetail(order.StagedOrderDetails[i]);
                    }
                }
                else if(args.Confidence > 0.9)
                {
                    message.UpdateText("Wat moet het zijn?");
                    order.StagedOrderDetails.Clear();
                }
                await _db.SaveChangesAsync();
                await StoreRecievedMessage(message);
            }
        }

        private async void r_DefaultResponse(object sender, EventArgs e)
        {
            var message = new Message();
            message.UpdateText("Sorry, ik kan alleen bestellingen afnemen. Hiervoor heb ik aantallen nodig. Wat wil je?");
            await StoreRecievedMessage(message);
        }

        private async Task BuildResponse(Message message, ModelResult modelResult, int quantity, Product product, ResponseNumberEventArgs eventArgs)
        {
            if (product == null)
            {
                message.Text = $"Sorry, {modelResult.Resolution.Values.First()} van wat";
                return;
            }
            else
            {
                message.Text = $"{message.Text} {quantity} {product.ProductName}";
            }


            if (eventArgs.numbers.IndexOf(modelResult) == eventArgs.numbers.Count - 1)
            {
                message.Text = $"{message.Text} als ik het goed begrijp?";
            }
            else if (eventArgs.numbers.Count > 1 && eventArgs.numbers.IndexOf(modelResult) == eventArgs.numbers.Count - 2)
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
}
