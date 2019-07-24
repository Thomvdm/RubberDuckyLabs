using RubberDucky.Common.Business.Interface;
using RubberDucky.Common.Data;
using RubberDucky.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RubberDucky.ConsoleApp
{
    public class Controller
    {
        private readonly AppDbContext _db;
        private readonly IDialog _orderDialog;
        private readonly IDialogResponseBuilder _orderDialogResponseBuilder;

        public Controller(AppDbContext db, IDialog orderDialog, IDialogResponseBuilder orderDialogResponseBuilder)
        {
            _db = db;
            _orderDialog = orderDialog;
            _orderDialogResponseBuilder = orderDialogResponseBuilder;
        }

        internal async Task ProcessRequest(string request, string orderId)
        {
            var message = new Message()
            {
                IsUser = true,
                Recieved = DateTime.Now,
            };
            message.UpdateText(request);
            await _orderDialog.ProcessMessage(message, orderId);
        }

        internal async Task<List<Message>> GetMessages()
        {
            return await _db.Set<Message>().AsNoTracking().ToListAsync();
        }

        internal async Task StoreRecievedMessage(string request)
        {
            var message = new Message()
            {
                IsUser = true,
                Recieved = DateTime.Now
            };
            message.UpdateText(request);
            await _orderDialogResponseBuilder.StoreRecievedMessage(message);
        }

        internal async Task<List<OrderDetail>> GetOrderDetails(string orderId)
        {
            return (await _db.Set<Order>().FirstAsync(x => x.OrderID == orderId)).ConfirmedOrderDetails;
        }
    }
}
