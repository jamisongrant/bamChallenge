using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Controllers;
using StargateAPI.Business.Queries;
using System.Threading.Tasks;
using System.Collections.Generic;
using StargateAPI.Business.Data;
using MediatR;
using StargateAPI.Business.Dtos;

namespace StargateAPI.Tests.Controllers
{
    [TestFixture]
    public class PersonControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private PersonController _controller;
        private List<PersonAstronaut> personList;

        [SetUp]
        public void SetUp()
        {
            personList = new List<PersonAstronaut>
            {
                new PersonAstronaut 
                { 
                    PersonId = 1, Name = "Jerry Seinfeld", 
                    CurrentRank = "Captain", 
                    CurrentDutyTitle = "Commander", 
                    CareerStartDate = DateTime.Now.AddYears(-10), 
                    CareerEndDate = DateTime.Now 
                }
            };
            _mediatorMock = new Mock<IMediator>();
            _controller = new PersonController(_mediatorMock.Object);
        }

        [Test]
        public async Task GetPeople_ReturnsOkResult_WithListOfPeople()
        {
            // Arrange
            var getPeopleResult = new GetPeopleResult { People = personList, Success = true };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPeople>(), default))
                          .ReturnsAsync(getPeopleResult);

            // Act
            var result = await _controller.GetPeople();

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(200));
            Assert.That(objectResult?.Value, Is.InstanceOf<GetPeopleResult>());

            var resultValue = objectResult?.Value as GetPeopleResult;
            Assert.That(resultValue?.People, Is.EqualTo(personList));
        }
    }
}
