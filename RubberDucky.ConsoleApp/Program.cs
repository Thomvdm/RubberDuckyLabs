using Microsoft.Extensions.DependencyInjection;
using RubberDucky.Common.Business;
using RubberDucky.Common.Business.Interface;
using RubberDucky.Common.Data;
using Microsoft.EntityFrameworkCore;
using System;
using RubberDucky.Common.Model;
using System.Threading.Tasks;

namespace RubberDucky.ConsoleApp
{
    class Program
    {
        private static IServiceProvider _serviceProvider;
        private static Controller _controller;
        private static int _messagePointer;
        private static bool _quitFlag;
        private static string _orderId;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate {
                _quitFlag = true;
                DisposeServices();
                Environment.Exit(0);
            };

            Initialize();

            while (!_quitFlag)
            {
                var request = Console.ReadLine();
                try
                {
                    StoreRecievedMessage(request).Wait();
                    ProcessRequest(request).Wait();
                    PrintNewMessages().Wait();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static async Task StoreRecievedMessage(string message)
        {
            await _controller.StoreRecievedMessage(message).ConfigureAwait(false);
            _messagePointer++;
        }

        private static async Task ProcessRequest(string request)
        {
            await _controller.ProcessRequest(request, _orderId).ConfigureAwait(false);
        }

        private static async Task PrintNewMessages()
        {
            var messages = await _controller.GetMessages().ConfigureAwait(false);
            var countMessagesAdd = 0;
            Console.ForegroundColor = ConsoleColor.Blue;
            for (var i = _messagePointer; i < messages.Count; i++)
            {
                Console.WriteLine(messages[i].Text);
                _messagePointer++;
                countMessagesAdd++;
            }
            Console.ForegroundColor = ConsoleColor.White;
            // There are 2 messages used to confirm the order input and asking for something else.
            // In this case the order is printed.
            if (countMessagesAdd > 1)
            {
                await PrintOrder();
            };
        }

        private static async Task PrintOrder()
        {
            Console.WriteLine("----------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var order in await _controller.GetOrderDetails(_orderId))
            {
                Console.WriteLine($"{order.Quantity} x {order.Product.ProductName}");
            }
            Console.WriteLine("----------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void Initialize()
        {
            _orderId = Guid.NewGuid().ToString();

            RegisterServices();
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Start loading data...");
            var start = DateTime.Now;
            DbInitializer.Initialize(_serviceProvider);
            Console.WriteLine($"Data loaded in {(DateTime.Now.Subtract(start)).TotalMilliseconds}ms");

            _controller = new Controller(_serviceProvider.GetService<AppDbContext>(), _serviceProvider.GetService<IDialog>(), _serviceProvider.GetService<IDialogResponseBuilder>());

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Quack Quack");
            Console.ForegroundColor = ConsoleColor.White;

            _messagePointer = 1;
        }

        private static void RegisterServices()
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                  options.UseInMemoryDatabase("AppDb"));

            services.AddTransient<IDialogResponseBuilder, OrderDialogResponseBuilder>();
            services.AddTransient<IDialog, OrderDialog>();
            services.AddTransient<IEntityRecognizer, EntityRecognizer>();
            _serviceProvider = services.BuildServiceProvider();
        }
        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}
