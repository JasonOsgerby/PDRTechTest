using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace PDR.PatientBooking.Service.BookingServices
{
    public interface IBookingService
    {
        void AddBooking(AddBookingRequest request);

        void CancelBooking(CancelBookingRequest bookingId);

        Order GetPatientNextBooking(long patientId);
    }
}