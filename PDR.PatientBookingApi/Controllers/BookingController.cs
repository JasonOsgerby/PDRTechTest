﻿using Microsoft.AspNetCore.Mvc;
using PDR.PatientBooking.Service.BookingServices;
using PDR.PatientBooking.Service.BookingServices.Requests;
using System;

namespace PDR.PatientBookingApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("patient/{patientId}/next")]
        public IActionResult GetPatientNextAppointment(long patientId)
        {
            var booking = _bookingService.GetPatientNextBooking(patientId);

            if (booking == null)
            {
                return StatusCode(502);
            }
            else
            {
                return Ok(new
                {
                    booking.Id,
                    booking.DoctorId,
                    booking.StartTime,
                    booking.EndTime
                });
            }
        }

        [HttpPost()]
        public IActionResult AddBooking(AddBookingRequest newBooking)
        {
            try
            {
                _bookingService.AddBooking(newBooking);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpDelete()]
        public IActionResult CancelBooking(CancelBookingRequest request)
        {
            try
            {
                _bookingService.CancelBooking(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}