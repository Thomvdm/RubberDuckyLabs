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

            
            // Determine which response to use. If found numbers respond to the numbers.
            if (numberResults.Count > 0)
            {
                UpdateBasedOnRecievedNumbers(numberResults, inputMessage.Words);
                await ImplicitConfirmation(numberResults, inputMessage.Words);
            }
            // If the confidence is high enough it could be that the user is confirming.
            if (confirmingConfidence > 0.5)
            {
                UpdateBasedOnConfirmation(isConfirming, confirmingConfidence);
                await Acknowledgement(isConfirming, confirmingConfidence);
            }

            // If there are none numbers found and user isn't confirming default resposne.
            if (numberResults.Count == 0 && confirmingConfidence < 0.5)
            {
                DefaultResponse();
            }            
        }
    }
}
