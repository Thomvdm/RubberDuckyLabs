using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using RubberDucky.Common.Model;
using RubberDucky.Common.Data;
using RubberDucky.Common.Business.Interface;
using RubberDucky.Common.Extensions;

namespace RubberDucky.Pages
{
    public partial class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IDialog _orderDialog;
        private readonly IDialogResponseBuilder _orderDialogResponseBuilder;

        public IndexModel(AppDbContext db, IDialog orderDialog, IDialogResponseBuilder orderDialogResponseBuilder)
        {
            _db = db;
            _orderDialog = orderDialog;
            _orderDialogResponseBuilder = orderDialogResponseBuilder;
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
            await _orderDialogResponseBuilder.StoreSendMessage(Message);
            await _orderDialog.ProcessMessage(Message, HttpContext?.Session?.GetString("OrderId"));
            return RedirectToPage();
        }
        #endregion
    }
}
