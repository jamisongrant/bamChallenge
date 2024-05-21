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
using Microsoft.EntityFrameworkCore;
using System.Linq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Services;

namespace StargateAPI.Tests.Controllers
{
    [TestFixture]
    public class PersonControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private Mock<IPersonService> _personServiceMock;
        private PersonController _controller;
        private List<PersonAstronaut> personList;
        private StargateContext _contextMock;

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

            var options = new DbContextOptionsBuilder<StargateContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _contextMock = new StargateContext(options);

            // Clear existing data
            _contextMock.People.RemoveRange(_contextMock.People);
            _contextMock.SaveChanges();

            // Add initial data
            _contextMock.People.AddRange(new List<Person>
            {
                new Person { Id = 1, Name = "Jerry Seinfeld" }
            });

            _contextMock.SaveChanges();

            _mediatorMock = new Mock<IMediator>();
            _personServiceMock = new Mock<IPersonService>();
            _controller = new PersonController(_mediatorMock.Object, _personServiceMock.Object);
        }

        // TearDown to release resources to avoid memory leaks from EFCore.InMemory
        [TearDown]
        public void TearDown()
        {
            _contextMock.Dispose();
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

        [Test]
        public async Task GetPersonByName_ReturnsOkResult_WithPerson()
        {
            // Arrange
            var personName = "Jerry Seinfeld";
            var personResult = new GetPersonByNameResult
            {
                Person = personList.First(),
                Success = true
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPersonByName>(), default))
                         .ReturnsAsync(personResult);

            // Act
            var result = await _controller.GetPersonByName(personName);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(200));
            Assert.That(objectResult?.Value, Is.InstanceOf<GetPersonByNameResult>());

            var resultValue = objectResult?.Value as GetPersonByNameResult;
            Assert.That(resultValue?.Person, Is.EqualTo(personList.First()));
        }

        [Test]
        public async Task CreatePerson_ReturnsOkResult_WithCreationResult()
        {
            // Arrange
            var personName = "Elaine Benes";
            var createPersonResult = new CreatePersonResult
            {
                Id = 2,
                Success = true
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<CreatePerson>(), default))
                         .ReturnsAsync(createPersonResult);

            // Act
            var result = await _controller.CreatePerson(personName);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(200));
            Assert.That(objectResult?.Value, Is.InstanceOf<CreatePersonResult>());

            var resultValue = objectResult?.Value as CreatePersonResult;
            Assert.That(resultValue?.Id, Is.EqualTo(2));
        }

        [Test]
        public async Task UpdatePerson_ReturnsOkResult_WithUpdateResult()
        {
            // Arrange
            var personName = "Jerry Seinfeld";
            var updateCommand = new UpdatePerson
            {
                Name = personName,
                NewName = "Newman"
            };
            var updatePersonResult = new BaseResponse
            {
                Success = true
            };
            _mediatorMock.Setup(m => m.Send(It.IsAny<UpdatePerson>(), default))
                         .ReturnsAsync(updatePersonResult);

            // Act
            var result = await _controller.UpdatePerson(personName, updateCommand);

            // Assert
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var objectResult = result as ObjectResult;
            Assert.That(objectResult?.StatusCode, Is.EqualTo(200));
            Assert.That(objectResult?.Value, Is.InstanceOf<BaseResponse>());

            var resultValue = objectResult?.Value as BaseResponse;
            Assert.That(resultValue?.Success, Is.True);
        }

        [Test]
        public async Task AddAstronautDetail_WithoutDuty_ShouldReturnBadRequest()
        {
            // Arrange
            var personId = 1;
            var astronautDetail = new AstronautDetail
            {
                CurrentDutyTitle = "Commander",
                CurrentRank = "Captain",
                CareerStartDate = DateTime.Now
            };

            _personServiceMock.Setup(s => s.AddAstronautDetail(personId, astronautDetail))
                .ThrowsAsync(new Exception("Person must have an AstronautDuty assignment before adding AstronautDetail"));

            // Act
            var result = await _controller.AddAstronautDetail(personId, astronautDetail);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult != null);

            // Check if the Value of BadRequestObjectResult is a dynamic object
            var badRequestValue = badRequestResult.Value;
            Assert.That(badRequestValue != null);

            // Convert the Value to a JSON string and then parse it
            var json = System.Text.Json.JsonSerializer.Serialize(badRequestValue);
            var parsed = System.Text.Json.JsonDocument.Parse(json);
            var message = parsed.RootElement.GetProperty("message").GetString();

            Assert.That(message, Is.EqualTo("Person must have an AstronautDuty assignment before adding AstronautDetail"));
        }



        [Test]
        public async Task AddAstronautDetail_WithDuty_ShouldReturnOk()
        {
            // Arrange
            var personId = 1;
            var astronautDetail = new AstronautDetail
            {
                CurrentDutyTitle = "Commander",
                CurrentRank = "Captain",
                CareerStartDate = DateTime.Now
            };

            _personServiceMock.Setup(s => s.AddAstronautDetail(personId, astronautDetail))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddAstronautDetail(personId, astronautDetail);

            // Assert
            Assert.That(result, Is.InstanceOf<OkResult>());
        }
    }
}
