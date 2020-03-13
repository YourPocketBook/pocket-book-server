using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PocketBookServer.Models
{
    public class RequestErrorConverter : JsonConverter<RequestErrorType>
    {
        public override RequestErrorType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var enumString = reader.GetString();

            return enumString switch
            {
                "isBlank" => RequestErrorType.IsBlank,
                "isInvalid" => RequestErrorType.IsInvalid,
                "isInUse" => RequestErrorType.IsInUse,
                _ => throw new InvalidOperationException(),
            };
        }

        public override void Write(Utf8JsonWriter writer, RequestErrorType value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case RequestErrorType.IsBlank:
                    writer.WriteStringValue("isBlank");
                    break;

                case RequestErrorType.IsInvalid:
                    writer.WriteStringValue("isInvalid");
                    break;

                case RequestErrorType.IsInUse:
                    writer.WriteStringValue("isInUse");
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
