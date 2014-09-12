namespace Helpful.EventStore.EventLogging
{
    public class StorableEvent
    {
        public string Stream { get; set; }
        public string Category { get; set; }
        public object Data { get; set; }
    }
}
