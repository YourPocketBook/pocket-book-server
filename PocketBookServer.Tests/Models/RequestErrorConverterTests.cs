using FluentAssertions;
using PocketBookServer.Models;
using System.Text.Json;
using Xunit;

namespace PocketBookServer.Tests.Models
{
    public class RequestErrorConverterTestClass
    {
        public RequestErrorType Error { get; set; }
    }

    public class RequestErrorConverterTests
    {
        [Theory, InlineData(RequestErrorType.IsBlank, "isBlank"), InlineData(RequestErrorType.IsInvalid, "isInvalid"),
            InlineData(RequestErrorType.IsInUse, "isInUse")]
        public void ConvertsToEnumCorrectly(RequestErrorType errorType, string input)
        {
            var json = $"{{\"Error\":\"{input}\"}}";

            var result = JsonSerializer.Deserialize<RequestErrorConverterTestClass>(json);
            result.Error.Should().Be(errorType);
        }

        [Theory, InlineData(RequestErrorType.IsBlank, "isBlank"), InlineData(RequestErrorType.IsInvalid, "isInvalid"),
            InlineData(RequestErrorType.IsInUse, "isInUse")]
        public void ConvertsToStringCorrectly(RequestErrorType errorType, string expected)
        {
            var obj = new RequestErrorConverterTestClass
            {
                Error = errorType
            };

            var result = JsonSerializer.Serialize(obj);

            result.Should().Be($"{{\"Error\":\"{expected}\"}}");
        }
    }
}