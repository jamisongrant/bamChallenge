using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateAPI.Controllers;

namespace StargateAPI.Tests.Controllers
{
    [TestFixture]
    public class AstronautDetailControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private AstronautDetailController _controller;
        private List<AstronautDetail> _astronautDetails;

        [SetUp]
        public void SetUp()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new AstronautDetailController(_mediatorMock.Object);
            _astronautDetails = new List<AstronautDetail>
            {
                new AstronautDetail
                {
                    PersonId = 1,
                    CurrentDutyTitle = "Commander",
                    CurrentRank = "Captain",
                    CareerStartDate = DateTime.Now.AddYears(-10)
                }
            };
        }

        [Test]
        public async Task GetAstronautDetails_ReturnsOkResult_WithListOfAstronautDetails()
        {
            // Arrange
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetAstronautDetailsQuery>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(_astronautDetails);

            // Act
            var result = await _controller.GetAstronautDetails();

            // Assert
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult?.StatusCode, Is.EqualTo(200));
            Assert.That(okResult?.Value, Is.InstanceOf<object>());

            var response = okResult?.Value;
            Assert.That(response.GetType().GetProperty("Success").GetValue(response), Is.EqualTo(true));
            Assert.That(response.GetType().GetProperty("Message").GetValue(response), Is.EqualTo("Astronaut details retrieved successfully"));
            Assert.That(response.GetType().GetProperty("Data").GetValue(response), Is.EqualTo(_astronautDetails));
        }

        [Test]
        public async Task CreateAstronautDetail_ReturnsOkResult_WithCreationResult()
        {
            // Arrange
            var command = new CreateAstronautDetail
            {
                PersonId = 1,
                CurrentDutyTitle = "Commander",
                CurrentRank = "Captain",
                CareerStartDate = DateTime.Now
            };

            var createResult = new BaseResponse
            {
                Success = true,
                Message = "AstronautDetail created successfully"
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateAstronautDetail>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(createResult);

            // Act
            var result = await _controller.CreateAstronautDetail(command);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var okResult = result as ObjectResult;
            Assert.That(okResult?.StatusCode, Is.EqualTo(200));
            Assert.That(okResult?.Value, Is.InstanceOf<BaseResponse>());

            var response = okResult?.Value as BaseResponse;
            Assert.That(response?.Success, Is.True);
            Assert.That(response?.Message, Is.EqualTo("AstronautDetail created successfully"));
        }

        [Test]
        public async Task CreateAstronautDetail_ReturnsBadRequest_OnException()
        {
            // Arrange
            var command = new CreateAstronautDetail
            {
                PersonId = 1,
                CurrentDutyTitle = "Commander",
                CurrentRank = "Captain",
                CareerStartDate = DateTime.Now
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreateAstronautDetail>(), It.IsAny<CancellationToken>()))
                         .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.CreateAstronautDetail(command);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var badRequestResult = result as ObjectResult;
            Assert.That(badRequestResult?.StatusCode, Is.EqualTo(500));
            Assert.That(badRequestResult?.Value, Is.InstanceOf<BaseResponse>());

            var response = badRequestResult?.Value as BaseResponse;
            Assert.That(response?.Success, Is.False);
            Assert.That(response?.Message, Is.EqualTo("Test exception"));
        }
    }
}
