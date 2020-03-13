using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Mvc;
using PocketBookServer.Models;
using System.Collections.Generic;
using System.Linq;

namespace PocketBookServer.Tests
{
    internal static class ActionResultExtensions
    {
        public static ActionResultAssertions Should(this IActionResult instance)
        {
            return new ActionResultAssertions(instance);
        }
    }

    internal class ActionResultAssertions : ReferenceTypeAssertions<IActionResult, ActionResultAssertions>
    {
        public ActionResultAssertions(IActionResult instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "action result";

        public AndConstraint<ActionResultAssertions> BeCreatedResultWithValue<T>(string path, T returnValue)
        {
            Subject.Should().BeOfType<CreatedResult>()
                .Which.Value.Should().BeEquivalentTo(returnValue);
            Subject.Should().BeOfType<CreatedResult>().Which.Location.Should().Be(path);

            return new AndConstraint<ActionResultAssertions>(this);
        }

        public AndConstraint<ActionResultAssertions> BeIsBlankResultForPath(string path)
        {
            BeIsErrorResultForPath(path, RequestErrorType.IsBlank);

            return new AndConstraint<ActionResultAssertions>(this);
        }

        public AndConstraint<ActionResultAssertions> BeIsBlankResultForPath(string[] path)
        {
            Subject.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<IList<RequestError>>()
                .Which.Should().BeEquivalentTo(path.Select(p => new RequestError { Error = RequestErrorType.IsBlank, Path = p }));

            return new AndConstraint<ActionResultAssertions>(this);
        }

        public AndConstraint<ActionResultAssertions> BeIsInUseResultForPath(string path)
        {
            BeIsErrorResultForPath(path, RequestErrorType.IsInUse);

            return new AndConstraint<ActionResultAssertions>(this);
        }

        public AndConstraint<ActionResultAssertions> BeIsInvalidResultForPath(string path)
        {
            BeIsErrorResultForPath(path, RequestErrorType.IsInvalid);

            return new AndConstraint<ActionResultAssertions>(this);
        }

        public AndConstraint<ActionResultAssertions> BeOkResultWithValue<T>(T returnValue)
        {
            Subject.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(returnValue);

            return new AndConstraint<ActionResultAssertions>(this);
        }

        private void BeIsErrorResultForPath(string path, RequestErrorType type)
        {
            Subject.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeAssignableTo<IList<RequestError>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new RequestError { Error = type, Path = path });
        }
    }
}
