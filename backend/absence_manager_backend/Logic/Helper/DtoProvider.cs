using AutoMapper;
using Entities.Dtos.AppUserDtos;
using Entities.Dtos.LocationDtos;
using Entities.Dtos.OfficeBooking;
using Entities.Dtos.OfficeDtos;
using Entities.Dtos.WorkStationDtos;
using Entities.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Helper
{
    public class DtoProvider
    {
        public IMapper Mapper { get; }

        public DtoProvider()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { });

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Location, LocationViewDto>();

                cfg.CreateMap<Office, OfficeViewDto>();

                cfg.CreateMap<Workstation, WorkstationViewDto>();

                cfg.CreateMap<AppUser, UserProfileDto>();

                cfg.CreateMap<CreateOfficeBookingDto, OfficeBooking>()
                    .ForMember(d => d.Id, o => o.Ignore())
                    .ForMember(d => d.UserId, o => o.Ignore())
                    .ForMember(d => d.CreatedAtUtc, o => o.Ignore())
                    .ForMember(d => d.CreatedByUserId, o => o.Ignore())
                    .ForMember(d => d.CancelledAtUtc, o => o.Ignore())
                    .ForMember(d => d.CancelledByUserId, o => o.Ignore())
                    .ForMember(d => d.IsCancelled, o => o.Ignore())
                    .ForMember(d => d.Workstation, o => o.Ignore())
                    .ForMember(d => d.User, o => o.Ignore());

                cfg.CreateMap<OfficeBooking, OfficeBookingViewDto>()
                    .ForMember(d => d.WorkstationCode, o => o.MapFrom(src => src.Workstation.Code))
                    .ForMember(d => d.WorkstationName, o => o.MapFrom(src => src.Workstation.Name))
                    .ForMember(d => d.OfficeId, o => o.MapFrom(src => src.Workstation.Office.Id))
                    .ForMember(d => d.OfficeName, o => o.MapFrom(src => src.Workstation.Office.Name))
                    .ForMember(d => d.LocationId, o => o.MapFrom(src => src.Workstation.Office.Location.Id))
                    .ForMember(d => d.LocationName, o => o.MapFrom(src => src.Workstation.Office.Location.Name))
                    .ForMember(d => d.UserName, o => o.MapFrom(src => src.User.DisplayName));
            }, loggerFactory);

            try
            {
                config.AssertConfigurationIsValid();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            Mapper = config.CreateMapper();
        }
    }
}
