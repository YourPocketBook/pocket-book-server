using System.Text.Json.Serialization;

namespace PocketBookServer.Models
{
    [JsonConverter(typeof(RequestErrorConverter))]
    public enum RequestErrorType
    {
        EmailNotConfirmed,
        IsBlank,
        IsInUse,
        IsInvalid
    }

    public class RequestError
    {
        public RequestErrorType Error { get; set; }
        public string Path { get; set; }
    }
}