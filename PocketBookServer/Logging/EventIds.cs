using Microsoft.Extensions.Logging;

namespace PocketBookServer.Logging
{
    public static class EventIds
    {
        public static readonly EventId ItemUpdated = new EventId(7001, "Item Updated");
        public static readonly EventId UnknownError = new EventId(9001, "Unknown Error");
        public static readonly EventId ValidationFailure = new EventId(1001, "Validation Failure");
    }
}
