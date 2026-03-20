using Entities.Dtos.Availability;
using Entities.Dtos.Helpers;
using Entities.Dtos.OfficeBooking;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Logic
{
    public interface IOfficeBookingLogic
    {
        Task<OfficeDayAvailabilityDto> GetOfficeDayAvailabilityAsync(int officeId, DateOnly date, string currentUserId);
        Task<List<DaySummaryDto>> GetOfficeDaySummariesAsync(int officeId, DateOnly fromDate, DateOnly toDate, string currentUserId);
        Task<OfficeBookingDto> CreateBookingAsync(CreateOfficeBookingDto dto, string currentUserId);
        Task CancelBookingAsync(int bookingId, string currentUserId);
        Task<List<OfficeBookingDto>> GetMyBookingsAsync(string currentUserId, DateOnly? fromDate, DateOnly? toDate);
    }
}
