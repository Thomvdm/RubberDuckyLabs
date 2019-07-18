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

        [BindProperty]
        public Message Message { get; set; }

        public IList<Message> Messages { get; private set; }

        public IList<Product> Products { get; private set; }

        public Order Order { get; private set; }

        public async Task OnGetAsync()
        {
            var orderId = HttpContext.Session.GetString("OrderId");
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
            Messages = await _db.Set<Message>().AsNoTracking().ToListAsync();
            Products = await _db.Set<Product>().AsNoTracking().ToListAsync();
            Order = await GetCurrentOrder();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            await StoreSendMessage(Message);
            Respond(Message);
            return RedirectToPage();
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

        private void Respond(Message inputMessage)
        {
            var responder = new Response();
            responder.RespondToNumber += r_OnRespondToNumber;
            responder.RespondToConfirmation += r_OnRespondToConfirmation;
            responder.DefaultResponse += r_DefaultResponse;
            responder.CheckOnNumber(inputMessage);
            responder.CheckOnConfirmation(inputMessage);
        }

        public List<OrderDetail> GetOrderDetails()
        {
            return Order.ConfirmedOrderDetails ?? new List<OrderDetail>();
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
    }
}
