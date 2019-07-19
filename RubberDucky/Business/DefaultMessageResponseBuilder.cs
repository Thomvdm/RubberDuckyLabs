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
    public class DefaultMessageResponseBuilder: IMessageResponseBuilder
    {
        private AppDbContext _db;
        private readonly IEntityRecognizer _entityRecognizer;

        public DefaultMessageResponseBuilder(AppDbContext db, IEntityRecognizer entityRecognizer)
        {
            _db = db;
            _entityRecognizer = entityRecognizer;
        }
        public async Task Acknowledgement(bool isConfirming, double confidence)
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

        public async Task ImplicitConfirmation(List<ModelResult> numbers, Dictionary<int, string> words)
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

                message.Text = $"{message.Text} {number.GetQuantity()} {likelyProduct.ProductName}";
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

        public async Task Prompt()
        {
            var message = new Message();
            message.UpdateText("Iets anders?");
            await StoreRecievedMessage(message);
        }

        public async Task FillPrompt()
        {
            var message = new Message();
            message.UpdateText("ja?");
            await StoreRecievedMessage(message);
        }

        public async Task FinalizePrompt()
        {
            var message = new Message();
            message.UpdateText("Komt eraan!");
            await StoreRecievedMessage(message);
        }

        public async void DefaultResponse()
        {
            var message = new Message();
            message.UpdateText("Ik kan alleen bestellingen afnemen. Hiervoor heb ik aantallen nodig. Wat wil je?");
            await StoreRecievedMessage(message);
        }

        public async Task StoreSendMessage(Message inputMessage)
        {
            inputMessage.UpdateText(inputMessage.Text);
            inputMessage.Id = Guid.NewGuid().ToString();
            inputMessage.IsUser = true;
            inputMessage.Recieved = DateTime.Now;
            _db.Messages.Add(inputMessage);
            await _db.SaveChangesAsync();
        }

        public async Task StoreRecievedMessage(Message message)
        {
            if (!string.IsNullOrWhiteSpace(message.Text))
            {
                message.Id = Guid.NewGuid().ToString();
                message.IsUser = false;
                message.Recieved = DateTime.Now;
                _db.Messages.Add(message);
                await _db.SaveChangesAsync();
            }
        }
    }
}
