using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBusConsole
{
    class RecieverService
    {
        // connection string to your Service Bus namespace
        static string connectionString = "<NAMESPACE CONNECTION STRING>";

        // name of your Service Bus queue
        static string queueName = "<QUEUE NAME>";

        // the sender used to publish messages to the queue
        static IQueueClient reciever;

        public RecieverService()
        {
            reciever = new QueueClient(connectionString, queueName, ReceiveMode.PeekLock);
        }

        /// <summary>
        /// Close the Queue after reading the messages
        /// </summary>
        /// <returns></returns>
        public Task CloseQueueMessage()
        {
            return reciever.CloseAsync();
        }

        /// <summary>
        /// Call the Read the Messages from the Queue method and close the Queue
        /// </summary>
        /// <returns></returns>
        public async Task ProcessMessagesFromQueue()
        {
            //Message Handler
            await Task.Run(() =>
            {
                RegisterOnMessageHandlerAndReceiveMessages();
                Console.ReadKey();
                CloseQueueMessage();
            });
        }

        public static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(LogException)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false,
                //set the time to renew the lock
                MaxAutoRenewDuration = TimeSpan.FromMinutes(5)
            };

            // Register the function that processes messages.
            reciever.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
            
        }

        public static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var messageBody = Encoding.UTF8.GetString(message.Body);
            // convert the json formatted message
            var messageServiceBus = JsonConvert.DeserializeObject(messageBody);

            try {
                // to do
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // Dead letter if fails
                await reciever.DeadLetterAsync(message.SystemProperties.LockToken);
            }

            //Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode(which is the default).
            await reciever.CompleteAsync(message.SystemProperties.LockToken);
        }

        public static Task LogException(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine("Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine("- Endpoint: {context.Endpoint}");
            Console.WriteLine("- Entity Path: {context.EntityPath}");
            Console.WriteLine("- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
