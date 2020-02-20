using Microsoft.Extensions.Logging;

namespace PocketBookServer.Logging
{
    public static class EventIds
    {
        public static readonly EventId EmailConfirmed = new EventId(5001, "User Email Confirmed");
        public static readonly EventId EmailNotConfirmed = new EventId(2001, "User Email Not Confirmed");
        public static readonly EventId EmailSent = new EventId(6001, "Email Sent");
        public static readonly EventId ItemUpdated = new EventId(7001, "Item Updated");
        public static readonly EventId PasswordChanged = new EventId(5003, "Validation Failure");
        public static readonly EventId UnknownError = new EventId(9001, "Unknown Error");
        public static readonly EventId UserCreated = new EventId(5002, "User Created");
        public static readonly EventId UserNotFound = new EventId(1002, "User Not Found");
        public static readonly EventId ValidationFailure = new EventId(1001, "Validation Failure");
    }
}