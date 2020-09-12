using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DashboardAsync.Models;
namespace DashboardAsync
{
    class Program
    {
        static string hostName = "127.0.0.1";
        static int portNumber = 8096;
        static IParser<IControlData> _parser;
        static void Main(string[] args)
        {
            _parser = new ParserApsTensileV1();
            //MessageHandler messageHandler = new MessageHandler();
            AsyncClient asyncClient = new AsyncClient(hostName,portNumber, new MessageHandler());
            asyncClient.MessageReceived += AsyncClient_MessageReceived;
            asyncClient.Connect();
            if (asyncClient.IsConnected)
            {
                asyncClient.Send("first data from program\r");
            }
            Console.ReadLine();
        }

        private static void AsyncClient_MessageReceived(object sender, string message)
        {
            IControlData data = _parser.Pars(message);
            if (data != null)
            {
                Console.WriteLine(data.Value.ToString());
            }
            //Console.WriteLine(message);
        }
    }
    class MessageHandler : IMessageHandler
    {
        public void HandleMessage(string message)
        {
            Console.WriteLine(message);
        }
        /*public async Task HandleMessage(string message)
        {
            await Task.Run(() => Console.WriteLine(message));
        }*/
    }
    //10000 message
    //local -> 21.74
    //event handler -> 22.86
    //event handler -> 22.50
    //event handler -> 22.65
    //event handler -> 22.77
    //sync handler -> 22.96
    //sync handler -> 22.46
    //sync handler -> 22.56
    //sync handler -> 25.18
    //sync handler -> 22.66
    //sync handler -> 22.84
    //async handler -> 26.72
    //async handler -> 26.39
    //async handler -> 26.58
}
