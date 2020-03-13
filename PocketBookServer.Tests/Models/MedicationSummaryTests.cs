using AutoFixture;
using FluentAssertions;
using PocketBookModel;
using PocketBookServer.Models;
using Xunit;

namespace PocketBookServer.Tests.Models
{
    public class MedicationSummaryTests
    {
        [Fact]
        public void PopulatesNameAndId()
        {
            var fixture = new Fixture();
            var testItem = fixture.Create<Medication>();

            var resultItem = new MedicationSummary(testItem);

            resultItem.Id.Should().Be(testItem.Id);
            resultItem.Name.Should().Be(testItem.Name);
        }
    }
}
