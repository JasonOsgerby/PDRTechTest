using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.DataSeed;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Validation;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Linq;

namespace PDR.PatientBooking.Service.Tests.BookingServices
{
    [TestFixture]
    public class BookingServiceTests
    {
        private MockRepository _mockRepository;
        private IFixture _fixture;

        private PatientBookingContext _context;
        private Mock<IAddBookingRequestValidator> _validator;

        private BookingService _bookingService;

        [SetUp]
        public void SetUp()
        {
            // Boilerplate
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _fixture = new Fixture();

            //Prevent fixture from generating circular references
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior(1));

            // Mock setup
            _context = new PatientBookingContext(new DbContextOptionsBuilder<PatientBookingContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _validator = _mockRepository.Create<IAddBookingRequestValidator>();

            new DatabaseSeed(_context).SeedDatabase();

            // Mock default
            SetupMockDefaults();

            // Sut instantiation
            _bookingService = new BookingService(
                _context,
                _validator.Object
            );
        }

        private void SetupMockDefaults()
        {
            _validator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>()))
                .Returns(new PdrValidationResult(true));

            _validator.Setup(x => x.ValidateRequest(It.IsAny<CancelBookingRequest>()))
                .Returns(new PdrValidationResult(true));
        }

        [Test]
        public void AddBooking_ValidatesRequest()
        {
            //arrange
            var request = new AddBookingRequest
            {
                PatientId = 100,
                DoctorId = 1,
                StartTime = DateTime.UtcNow.AddHours(3),
                EndTime = DateTime.UtcNow.AddHours(4)
            };
            
            //act
            _bookingService.AddBooking(request);

            //assert
            _validator.Verify(x => x.ValidateRequest(request), Times.Once);
        }

        [Test]
        public void AddBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            var request = new AddBookingRequest
            {
                PatientId = 100,
                DoctorId = 1,
                StartTime = DateTime.UtcNow.AddHours(3),
                EndTime = DateTime.UtcNow.AddHours(4)
            };

            _validator.Setup(x => x.ValidateRequest(It.IsAny<AddBookingRequest>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.AddBooking(request));

            //assert
            exception.Message.Should().Be(failedValidationResult.Errors.First());
        }

        [Test]
        public void AddBooking_AddsOrderToContextWithGeneratedId()
        {
            //arrange
            var request = new AddBookingRequest
            {
                PatientId = 100,
                DoctorId = 1,
                StartTime = DateTime.UtcNow.AddHours(3),
                EndTime = DateTime.UtcNow.AddHours(4)
            };

            var expected = new Order
            {
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };

            //act
            _bookingService.AddBooking(request);

            //assert
            _context.Order.Should().ContainEquivalentOf(expected, 
                options => options.Including(order => order.PatientId)
                .Including(order => order.DoctorId)
                .Including(order => order.StartTime)
                .Including(order => order.EndTime)
            );
        }

        [Test]
        public void CancelBooking_ValidatesRequest()
        {
            //arrange
            var id = Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9");

            var request = new CancelBookingRequest
            {
                BookingId = id
            };

            //act
            _bookingService.CancelBooking(request);

            //assert
            _validator.Verify(x => x.ValidateRequest(request), Times.Once);
        }

        [Test]
        public void CancelBooking_ValidatorFails_ThrowsArgumentException()
        {
            //arrange
            var failedValidationResult = new PdrValidationResult(false, _fixture.Create<string>());

            var id = Guid.Parse("00000000-0000-0000-0000-000000000000");

            var request = new CancelBookingRequest
            {
                BookingId = id
            };

            _validator.Setup(x => x.ValidateRequest(It.IsAny<CancelBookingRequest>())).Returns(failedValidationResult);

            //act
            var exception = Assert.Throws<ArgumentException>(() => _bookingService.CancelBooking(request));

            //assert
            exception.Message.Should().Be(failedValidationResult.Errors.First());
        }

        [Test]
        public void CancelBooking_MarksOrderAsCancelled()
        {
            //arrange
            var id = Guid.Parse("683074b8-44c9-468b-9288-dfafa1e533c9");

            var request = new CancelBookingRequest
            {
                BookingId = id
            };

            var expected = new Order
            {
                Id = id,
                Cancelled = true
            };

            //act
            _bookingService.CancelBooking(request);

            //assert
            _context.Order.Should().ContainEquivalentOf(expected,
               options => options.Including(order => order.Id)
               .Including(order => order.Cancelled)
           );
        }

        [Test]
        public void GetPatientNextAppointment_RetrievesCorrectBooking()
        {
            //arrange
            var booking = new AddBookingRequest
            {
                PatientId = 100,
                DoctorId = 1,
                StartTime = DateTime.UtcNow.AddHours(24),
                EndTime = DateTime.UtcNow.AddHours(25)
            };

            _bookingService.AddBooking(booking);

            //act
            var order = _bookingService.GetPatientNextBooking(100);

            //assert
            Assert.AreEqual(order.PatientId, booking.PatientId);
            Assert.AreEqual(order.DoctorId, booking.DoctorId);
            Assert.AreEqual(order.StartTime, booking.StartTime);
            Assert.AreEqual(order.EndTime, booking.EndTime);
        }
    }
}