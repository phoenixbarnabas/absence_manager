using Data;
using Entities.Dtos.Availability;
using Entities.Dtos.Helpers;
using Entities.Dtos.OfficeBooking;
using Entities.Models;
using Logic.Helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic.Logic
{
    public class OfficeBookingLogic
    {
        private readonly Repository<OfficeBooking> _officeBookingRepository;
        private readonly Repository<Workstation> _workstationRepository;
        private readonly Repository<Office> _officeRepository;
        private readonly Repository<Location> _locationRepository;
        private readonly Repository<AppUser> _appUserRepository;
        private readonly DtoProvider _dtoProvider;

        public OfficeBookingLogic(
            Repository<OfficeBooking> officeBookingRepository,
            Repository<Workstation> workstationRepository,
            Repository<Office> officeRepository,
            Repository<Location> locationRepository,
            Repository<AppUser> appUserRepository,
            DtoProvider dtoProvider)
        {
            _officeBookingRepository = officeBookingRepository;
            _workstationRepository = workstationRepository;
            _officeRepository = officeRepository;
            _locationRepository = locationRepository;
            _appUserRepository = appUserRepository;
            _dtoProvider = dtoProvider;
        }

        public OfficeDayAvailabilityDto GetOfficeDayAvailability(string officeId, DateOnly date, string currentUserId)
        {
            ValidateBookingDate(date);

            var office = _officeRepository.GetAll()
                .Include(o => o.Location)
                .FirstOrDefault(o => o.Id == officeId);

            if (office == null)
                throw new KeyNotFoundException("Office not found.");

            if (!office.IsActive)
                throw new InvalidOperationException("Office is not active.");

            if (!office.Location.IsActive)
                throw new InvalidOperationException("Location is not active.");

            var workstations = _workstationRepository.GetAll()
                .Where(w => w.OfficeId == officeId && w.IsActive)
                .OrderBy(w => w.DisplayOrder)
                .ToList();

            var bookings = _officeBookingRepository.GetAll()
                .Include(b => b.Workstation)
                .Where(b =>
                    b.Workstation.OfficeId == officeId &&
                    b.BookingDate == date &&
                    !b.IsCancelled)
                .ToList();

            var workstationDtos = workstations
                .Select(w =>
                {
                    var booking = bookings.FirstOrDefault(b => b.WorkstationId == w.Id);

                    return new WorkstationAvailabilityDto
                    {
                        WorkstationId = w.Id,
                        WorkstationCode = w.Code,
                        WorkstationName = w.Name,
                        DisplayOrder = w.DisplayOrder,
                        IsActive = w.IsActive,
                        IsBooked = booking != null,
                        IsBookedByCurrentUser = booking != null && booking.AppUserId == currentUserId,
                        BookingId = booking?.Id,
                        BookedByUserId = booking?.AppUserId,
                        BookedByUserName = booking?.AppUser?.DisplayName,
                        PositionX = w.PositionX,
                        PositionY = w.PositionY
                    };
                })
                .ToList();

            var currentUserBooking = bookings.FirstOrDefault(b => b.AppUserId == currentUserId);

            return new OfficeDayAvailabilityDto
            {
                BookingDate = date,
                OfficeId = office.Id,
                OfficeName = office.Name,
                LocationId = office.LocationId,
                LocationName = office.Location.Name,
                TotalWorkstations = workstationDtos.Count,
                BookedWorkstations = workstationDtos.Count(x => x.IsBooked),
                FreeWorkstations = workstationDtos.Count(x => !x.IsBooked),
                CurrentUserHasBooking = currentUserBooking != null,
                CurrentUserBookingId = currentUserBooking?.Id,
                CurrentUserWorkstationId = currentUserBooking?.WorkstationId,
                Workstations = workstationDtos
            };
        }

        public IEnumerable<DaySummaryDto> GetOfficeDaySummaries(string officeId, DateOnly fromDate, DateOnly toDate, string currentUserId)
        {
            if (toDate < fromDate)
                throw new ArgumentException("toDate cannot be earlier than fromDate.");

            ValidateBookingDate(fromDate);
            ValidateBookingDate(toDate);

            var office = _officeRepository.FindById(officeId);
            if (office == null)
                throw new KeyNotFoundException("Office not found.");

            var totalWorkstations = _workstationRepository.GetAll()
                .Count(w => w.OfficeId == officeId && w.IsActive);

            var bookings = _officeBookingRepository.GetAll()
                .Include(b => b.Workstation)
                .Where(b =>
                    b.Workstation.OfficeId == officeId &&
                    b.BookingDate >= fromDate &&
                    b.BookingDate <= toDate &&
                    !b.IsCancelled)
                .ToList();

            var result = new List<DaySummaryDto>();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                var dayBookings = bookings.Where(b => b.BookingDate == date).ToList();

                result.Add(new DaySummaryDto
                {
                    Date = date,
                    TotalWorkstations = totalWorkstations,
                    BookedWorkstations = dayBookings.Count,
                    FreeWorkstations = totalWorkstations - dayBookings.Count,
                    CurrentUserHasBooking = dayBookings.Any(b => b.AppUserId == currentUserId)
                });
            }

            return result;
        }

        public OfficeBookingViewDto CreateBooking(CreateOfficeBookingDto dto, string currentUserId)
        {
            ValidateBookingDate(dto.BookingDate);

            var user = _appUserRepository.FindById(currentUserId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!user.IsActive)
                throw new InvalidOperationException("User is not active.");

            var workstation = _workstationRepository.GetAll()
                .Include(w => w.Office)
                .ThenInclude(o => o.Location)
                .FirstOrDefault(w => w.Id == dto.WorkstationId);

            if (workstation == null)
                throw new KeyNotFoundException("Workstation not found.");

            if (!workstation.IsActive)
                throw new InvalidOperationException("Workstation is not active.");

            if (!workstation.Office.IsActive)
                throw new InvalidOperationException("Office is not active.");

            if (!workstation.Office.Location.IsActive)
                throw new InvalidOperationException("Location is not active.");

            var userAlreadyHasBooking = _officeBookingRepository.GetAll()
                .Any(b => b.AppUserId == currentUserId && b.BookingDate == dto.BookingDate && !b.IsCancelled);

            if (userAlreadyHasBooking)
                throw new InvalidOperationException("The user already has a booking for this date.");

            var workstationAlreadyBooked = _officeBookingRepository.GetAll()
                .Any(b => b.WorkstationId == dto.WorkstationId && b.BookingDate == dto.BookingDate && !b.IsCancelled);

            if (workstationAlreadyBooked)
                throw new InvalidOperationException("This workstation is already booked for the selected date.");

            var booking = _dtoProvider.Mapper.Map<OfficeBooking>(dto);

            booking.AppUserId = user.Id;
            booking.CreatedAtUtc = DateTime.UtcNow;
            booking.CreatedByUserId = currentUserId;
            booking.IsCancelled = false;

            _officeBookingRepository.Add(booking);

            var createdBooking = _officeBookingRepository.GetAll()
                .Include(b => b.Workstation)
                .ThenInclude(w => w.Office)
                .ThenInclude(o => o.Location)
                .Include(b => b.AppUser)
                .FirstOrDefault(b => b.Id == booking.Id);

            if (createdBooking == null)
                throw new InvalidOperationException("Created booking could not be loaded.");

            return _dtoProvider.Mapper.Map<OfficeBookingViewDto>(createdBooking);
        }

        public IEnumerable<OfficeBookingViewDto> GetMyBookings(string currentUserId, DateOnly? fromDate = null, DateOnly? toDate = null)
        {
            var query = _officeBookingRepository.GetAll()
                .Include(b => b.Workstation)
                .ThenInclude(w => w.Office)
                .ThenInclude(o => o.Location)
                .Include(b => b.AppUser)
                .Where(b => b.AppUserId == currentUserId && !b.IsCancelled);

            if (fromDate.HasValue)
                query = query.Where(b => b.BookingDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(b => b.BookingDate <= toDate.Value);

            return query
                .OrderBy(b => b.BookingDate)
                .ToList()
                .Select(b => _dtoProvider.Mapper.Map<OfficeBookingViewDto>(b));
        }

        public void CancelBooking(string bookingId, string currentUserId, bool isAdmin = false)
        {
            var booking = _officeBookingRepository.GetAll()
                .Include(b => b.Workstation)
                .FirstOrDefault(b => b.Id == bookingId);

            if (booking == null)
                throw new KeyNotFoundException("Booking not found.");

            if (booking.IsCancelled)
                throw new InvalidOperationException("Booking is already cancelled.");

            if (!isAdmin && booking.AppUserId != currentUserId)
                throw new UnauthorizedAccessException("You can only cancel your own booking.");

            var today = DateOnly.FromDateTime(DateTime.Today);

            if (!isAdmin && booking.BookingDate <= today)
                throw new InvalidOperationException("Same-day bookings cannot be cancelled by the user.");

            booking.IsCancelled = true;
            booking.CancelledAtUtc = DateTime.UtcNow;
            booking.CancelledByUserId = currentUserId;

            _officeBookingRepository.Update(booking);
        }

        private static void ValidateBookingDate(DateOnly bookingDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var maxDate = today.AddDays(14);

            if (bookingDate < today)
                throw new InvalidOperationException("Booking date cannot be in the past.");

            if (bookingDate > maxDate)
                throw new InvalidOperationException("Booking date cannot be more than 14 days in advance.");
        }
    }
}