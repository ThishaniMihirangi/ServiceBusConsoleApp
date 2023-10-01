using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace ServiceBusConsole
{
    class SenderService
    {
        // connection string to your Service Bus namespace
        static string connectionString = "<NAMESPACE CONNECTION STRING>";

        // name of your Service Bus queue
        static string queueName = "<QUEUE NAME>";

        // the sender used to publish messages to the queue
        static IQueueClient sender;


        public SenderService()
        {
            sender = new QueueClient(connectionString, queueName);
        }

        public Task CloseQueueMessage()
        {
            return sender.CloseAsync();
        }

        public Task SendToQueue()
        {

            var message = "{ \"FirstName\":\"John\",\"LastName\":\"Doe\" }";

            string messageId = Guid.NewGuid().ToString();
            var serializeBody = JsonConvert.SerializeObject(message, Formatting.Indented);

            var messageServiceBus = new Message(Encoding.UTF8.GetBytes(serializeBody));
            messageServiceBus.MessageId = messageId;

            return sender.SendAsync(messageServiceBus);
        }
    }
}
