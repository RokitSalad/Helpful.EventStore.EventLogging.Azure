using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Helpful.EventStore.EventLogging.Service
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string QueueName = "EventLogging";

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient _client;
        readonly ManualResetEvent _completedEvent = new ManualResetEvent(false);
        private StoreService _storeSerivce;

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.
            _client.OnMessage((receivedMessage) =>
                {
                    try
                    {
                        // Process the message
                        Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());

                        _storeSerivce.StoreEvent(receivedMessage.GetBody<StorableEvent>());
                    }
                    catch(Exception e)
                    {
                        Trace.TraceError(e.ToString());
                    }
                });

            _completedEvent.WaitOne();
        }

        public override bool OnStart()
        {
            Initialise();

            return base.OnStart();
        }

        private void Initialise()
        {
            CreateQueue();

            InitialiseStoreService();
        }

        private void InitialiseStoreService()
        {
            int port;
            if (!int.TryParse(CloudConfigurationManager.GetSetting("EventStorePort"), out port))
            {
                throw new ConfigurationErrorsException("Port number will not convert to integer.");
            }
            _storeSerivce =
                StoreServiceFactory.BuildStoreService(CloudConfigurationManager.GetSetting("EventStoreUsername"),
                    CloudConfigurationManager.GetSetting("EventStorePassword"),
                    CloudConfigurationManager.GetSetting("IpAddress"),
                    port, true);
        }

        private void CreateQueue()
        {
// Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            if (!namespaceManager.QueueExists(QueueName))
            {
                namespaceManager.CreateQueue(QueueName);
            }

            // Initialize the connection to Service Bus Queue
            _client = QueueClient.CreateFromConnectionString(connectionString, QueueName);
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            _client.Close();
            _completedEvent.Set();
            base.OnStop();
        }
    }
}
