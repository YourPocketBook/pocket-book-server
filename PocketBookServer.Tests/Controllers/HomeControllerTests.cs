using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PocketBookServer.Controllers;
using Xunit;

namespace PocketBookServer.Tests.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public void ReturnsUIOnCall()
        {
            var controller = new HomeController();

            var result = controller.Index();

            result.Should().BeOfType<VirtualFileResult>();

            var res = (VirtualFileResult)result;

            res.FileName.Should().Be("~/index.html");
            res.ContentType.Should().Be("text/html");
        }
    }
}