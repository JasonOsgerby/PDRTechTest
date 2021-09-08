using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class AddBookingRequestValidator : IAddBookingRequestValidator
    {
        private readonly PatientBookingContext _context;

        public AddBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(AddBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            if (CheckPatientExists(request, ref result))
                return result;

            if (CheckBookingNotInThePast(request, ref result))
                return result;

            if (CheckDoctorNotAlreadyBooked(request, ref result))
                return result;

            return result;
        }

        private bool CheckPatientExists(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (_context.Order.Any(o => o.PatientId == request.PatientId))
            {
                result.PassedValidation = false;
                result.Errors.Add("Specified patient does not exist");
                return true;
            }

            return false;
        }

        private bool CheckBookingNotInThePast(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (request.StartTime < DateTime.UtcNow)
            {
                result.PassedValidation = false;
                result.Errors.Add("Specified booking start time is in the past");
                return true;
            }
            return false;
        }

        private bool CheckDoctorNotAlreadyBooked(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (_context.Order.Any(o => o.DoctorId == request.DoctorId && o.StartTime >= request.StartTime && o.EndTime <= request.EndTime))
            {
                result.PassedValidation = false;
                result.Errors.Add("Doctor already booked between specified start and end times");
                return true;
            }

            return false;
        }
    }
}
