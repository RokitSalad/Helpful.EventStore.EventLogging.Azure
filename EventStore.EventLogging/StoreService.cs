using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;

namespace EventStore.EventLogging
{
    public class StoreService
    {
        private readonly UserCredentials _credentials;
        private readonly IEventStoreConnection _connection;

        public StoreService(UserCredentials credentials, IEventStoreConnection connection)
        {
            _credentials = credentials;
            _connection = connection;
        }

        public void StoreEvent(StorableEvent sEvent)
        {
            _connection.AppendToStreamAsync(string.Format("{0}_{1}", sEvent.Stream, sEvent.Category),
                ExpectedVersion.Any, _credentials, sEvent.Data.AsJson());
        }
    }
}
