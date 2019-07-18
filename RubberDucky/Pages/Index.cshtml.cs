using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RubberDucky.Data;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;
using RubberDucky.Business;
using RubberDucky.Model;
using Microsoft.AspNetCore.Http;
using System.Text;
using Newtonsoft.Json;
using RubberDucky.Extensions;

namespace RubberDucky.Pages
{
    public partial class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly EntityRecognizer _entityRecognizer;

        public IndexModel(AppDbContext db)
        {
            _db = db;
            _entityRecognizer = new EntityRecognizer(db);
        }

        #region Data
        [BindProperty]
        public Message Message { get; set; }

        public IList<Message> Messages { get; private set; }

        public IList<Product> Products { get; private set; }

        public Order Order { get; private set; }

        public async Task<List<OrderDetail>> GetOrderDetails()
        {
            return (await GetCurrentOrder()).ConfirmedOrderDetails ?? new List<OrderDetail>();
        }

        public async Task<Order> GetCurrentOrder()
        {
            return await _db.Set<Order>()
                    .Include(o => o.ConfirmedOrderDetails)
                    .ThenInclude(od => od.Product)
                    .Include(o => o.StagedOrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(x => x.OrderID.Equals(HttpContext.Session.GetString("OrderId")));
        }

        public async Task<IList<Product>> GetProducts()
        {
            if (Products == null)
            {
                Products = await _db.Set<Product>().AsNoTracking().ToListAsync();
            }
            return Products;
        }

        public async Task<IList<Message>> GetMessages()
        {
            if (Messages == null)
            {
                Messages = await _db.Set<Message>().AsNoTracking().ToListAsync();
            }
            return Messages;
        }

        private async Task StoreSendMessage(Message inputMessage)
        {
            inputMessage.UpdateText(inputMessage.Text);
            inputMessage.Id = Guid.NewGuid().ToString();
            inputMessage.IsUser = true;
            inputMessage.Recieved = DateTime.Now;
            _db.Messages.Add(inputMessage);
            await _db.SaveChangesAsync();
        }

        private async Task StoreRecievedMessage(Message message)
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
        #endregion

        #region Actions
        public async Task OnGetAsync()
        {
            var orderId = HttpContext.Session.GetString("OrderId");
            // Check if orderid is set for the session. If not start the order en initialize an orderID
            if (string.IsNullOrWhiteSpace(orderId))
            {
                HttpContext.Session.Set("OrderId", Guid.NewGuid().ToString());
                orderId = HttpContext.Session.GetString("OrderId");
                var order = new Order()
                {
                    OrderID = orderId,
                    OrderDate = DateTime.Now
                };
                _db.Set<Order>().Add(order);
                await _db.SaveChangesAsync();
            }

        }

        // Method called when the user sends a message
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Message.Text))
            {
                return Page();
            }
            await StoreSendMessage(Message);
            await ProcessMessage(Message);
            return RedirectToPage();
        }
        #endregion

        private async Task ProcessMessage(Message inputMessage)
        {
            List<ModelResult> numberResults;
            bool isConfirming;
            double confirmingConfidence;
            //To multithreading? Or Task?
            inputMessage.CheckOnNumber(out numberResults);
            inputMessage.CheckConformation(out isConfirming, out confirmingConfidence);

            var processedStaged = false;
            // If the confidence is high enough it could be that the user is confirming.
            if (confirmingConfidence > 0.5 && (await GetCurrentOrder()).StagedOrderDetails.Count > 0)
            {
                UpdateBasedOnConfirmation(isConfirming, confirmingConfidence);
                await Acknowledgement(isConfirming, confirmingConfidence);
                processedStaged = true;
            }
            // Determine which response to use. If found numbers respond to the numbers.
            if (numberResults.Count > 0)
            {
                UpdateBasedOnRecievedNumbers(numberResults, inputMessage.Words);
                await ImplicitConfirmation(numberResults, inputMessage.Words);
            }
            // Prompt for something else
            if ((await GetCurrentOrder()).StagedOrderDetails.Count == 0 && processedStaged)
            {
                Prompt();
            } else if(!isConfirming && confirmingConfidence > 0.9 && !processedStaged)
            {
                FinalizePrompt();
            } else if(isConfirming && !processedStaged)
            {
                FillPrompt();
            }

            // If there are none numbers found and user isn't confirming default resposne.
            if (numberResults.Count == 0 && confirmingConfidence < 0.5)
            {
                DefaultResponse();
            } 
        }

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
    }
}
