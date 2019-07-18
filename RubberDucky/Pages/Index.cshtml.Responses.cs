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
        private async Task Acknowledgement(bool isConfirming, double confidence)
        {
            var message = new Message();
            if (isConfirming)
            {
                message.UpdateText("Staat genoteerd.");
            }
            else if (confidence > 0.9)
            {
                message.UpdateText("Wat moet het zijn?");
            }
            await StoreRecievedMessage(message);
        }

        private async Task ImplicitConfirmation(List<ModelResult> numbers, Dictionary<int, string> words)
        {
            var message = new Message();
            foreach (var number in numbers)
            {
                var likelyProduct = _entityRecognizer.GetProducts(words, number).FirstOrDefault();
                // If the number can't relate to a product from the menu ask to specify
                if (likelyProduct == null)
                {
                    message.Text = $"{number.Resolution.Values.First()} van wat?";
                    return;
                }

                message.Text = $"{message.Text} {GetQuantity(number)} {likelyProduct.ProductName}";
                // If number is last in line round up sentence
                if (numbers.IndexOf(number) == numbers.Count - 1)
                {
                    message.Text = $"{message.Text} als ik het goed begrijp?";
                }
                // If number is second last in line descriptive addition of the sommation
                else if (numbers.Count > 1 && numbers.IndexOf(number) == numbers.Count - 2)
                {
                    message.Text = $"{message.Text} en";
                }
                // Else comma seperate the sommation
                else
                {
                    message.Text = $"{message.Text}, ";
                }
                await StoreRecievedMessage(message);
            }
            
        }

        private async Task Prompt()
        {
            var message = new Message();
            message.UpdateText("Iets anders?");
            await StoreRecievedMessage(message);
        }

        private async Task FillPrompt()
        {
            var message = new Message();
            message.UpdateText("ja?");
            await StoreRecievedMessage(message);
        }

        private async Task FinalizePrompt()
        {
            var message = new Message();
            message.UpdateText("Komt eraan!");
            await StoreRecievedMessage(message);
        }

        private async void DefaultResponse()
        {
            var message = new Message();
            message.UpdateText("Ik kan alleen bestellingen afnemen. Hiervoor heb ik aantallen nodig. Wat wil je?");
            await StoreRecievedMessage(message);
        }
    }
}
