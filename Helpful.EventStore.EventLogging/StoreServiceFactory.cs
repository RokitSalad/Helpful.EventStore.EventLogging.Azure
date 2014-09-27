using System.Net;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace Helpful.EventStore.EventLogging
{
    public static class StoreServiceFactory
    {
        private static StoreService _service;
        public static StoreService BuildStoreService(string username, string password, string ipAddress, int port, bool forceReconnect = false)
        {
            if(_service == null || forceReconnect)
            {
                IEventStoreConnection connection = EventStoreConnection.Create(new IPEndPoint(IPAddress.Parse(ipAddress), port));
                UserCredentials credentials = new UserCredentials(username, password);
                _service = new StoreService(credentials, connection);
            }
            return _service;
        }
    }
}
