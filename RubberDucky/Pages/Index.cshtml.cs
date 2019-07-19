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
using RubberDucky.Business.Interface;
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
        private readonly IMessageProcessor _defaultMessageProcessor;
        private readonly IMessageResponseBuilder _defaultMessageResponseBuilder;

        public IndexModel(AppDbContext db, IMessageProcessor defaultMessageProcessor, IMessageResponseBuilder defaultMessageResponseBuilder)
        {
            _db = db;
            _defaultMessageProcessor = defaultMessageProcessor;
            _defaultMessageResponseBuilder = defaultMessageResponseBuilder;
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
            if (Order == null)
            {
                Order = await _db.GetCurrentOrder(HttpContext.Session.GetString("OrderId"));
            }
            return Order;
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
            _defaultMessageProcessor.SetOrderId(HttpContext?.Session?.GetString("OrderId"));
            await _defaultMessageResponseBuilder.StoreSendMessage(Message);
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
            var orderId = HttpContext.Session.GetString("OrderId");
            var processedStaged = false;
            // If the confidence is high enough it could be that the user is confirming.
            if (confirmingConfidence > 0.5 && (await GetCurrentOrder()).StagedOrderDetails.Count > 0)
            {
                _defaultMessageProcessor.UpdateBasedOnConfirmation(isConfirming, confirmingConfidence);
                await _defaultMessageResponseBuilder.Acknowledgement(isConfirming, confirmingConfidence);
                processedStaged = true;
            }
            // Determine which response to use. If found numbers respond to the numbers.
            if (numberResults.Count > 0)
            {
                _defaultMessageProcessor.UpdateBasedOnRecievedNumbers(numberResults, inputMessage.Words);
                await _defaultMessageResponseBuilder.ImplicitConfirmation(numberResults, inputMessage.Words);
            }
            // Prompt for something else
            if ((await GetCurrentOrder()).StagedOrderDetails.Count == 0 && processedStaged)
            {
                _defaultMessageResponseBuilder.Prompt();
            } else if(!isConfirming && confirmingConfidence > 0.9 && !processedStaged)
            {
                _defaultMessageResponseBuilder.FinalizePrompt();
            } else if(isConfirming && !processedStaged)
            {
                _defaultMessageResponseBuilder.FillPrompt();
            }

            // If there are none numbers found and user isn't confirming default resposne.
            if (numberResults.Count == 0 && confirmingConfidence < 0.5)
            {
                _defaultMessageResponseBuilder.DefaultResponse();
            } 
        }
    }
}
