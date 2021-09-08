using System;

namespace PDR.PatientBooking.Service.BookingServices.Requests
{
    public class CancelBookingRequest
    {
        public Guid BookingId { get; set; }
    }
}
