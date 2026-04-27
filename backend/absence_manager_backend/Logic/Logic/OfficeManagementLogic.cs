using Data;
using Entities.Dtos.LocationDtos;
using Entities.Dtos.OfficeDtos;
using Entities.Dtos.WorkStationDtos;
using Entities.Models;
using Logic.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic.Logic
{
    public class OfficeManagementLogic
    {
        private readonly Repository<Location> _locationRepository;
        private readonly Repository<Office> _officeRepository;
        private readonly Repository<Workstation> _workstationRepository;
        private readonly DtoProvider _dtoProvider;

        public OfficeManagementLogic(
            Repository<Location> locationRepository,
            Repository<Office> officeRepository,
            Repository<Workstation> workstationRepository,
            DtoProvider dtoProvider)
        {
            _locationRepository = locationRepository;
            _officeRepository = officeRepository;
            _workstationRepository = workstationRepository;
            _dtoProvider = dtoProvider;
        }

        public IEnumerable<LocationViewDto> GetLocations(bool activeOnly = false)
        {
            var query = _locationRepository.GetAll();

            if (activeOnly)
                query = query.Where(x => x.IsActive);

            return query
                .OrderBy(x => x.DisplayOrder)
                .ToList()
                .Select(x => _dtoProvider.Mapper.Map<LocationViewDto>(x));
        }

        public IEnumerable<OfficeViewDto> GetOfficesByLocation(string locationId, bool activeOnly = false)
        {
            _locationRepository.FindById(locationId);

            var query = _officeRepository.GetAll()
                .Where(x => x.LocationId == locationId);

            if (activeOnly)
                query = query.Where(x => x.IsActive);

            return query
                .OrderBy(x => x.DisplayOrder)
                .ToList()
                .Select(x => _dtoProvider.Mapper.Map<OfficeViewDto>(x));
        }

        public IEnumerable<WorkstationViewDto> GetWorkstationsByOffice(string officeId, bool activeOnly = false)
        {
            _officeRepository.FindById(officeId);

            var query = _workstationRepository.GetAll()
                .Where(x => x.OfficeId == officeId);

            if (activeOnly)
                query = query.Where(x => x.IsActive);

            return query
                .OrderBy(x => x.DisplayOrder)
                .ToList()
                .Select(x => _dtoProvider.Mapper.Map<WorkstationViewDto>(x));
        }

        public WorkstationViewDto GetWorkstationById(string workstationId)
        {
            var workstation = _workstationRepository.FindById(workstationId);
            return _dtoProvider.Mapper.Map<WorkstationViewDto>(workstation);
        }
    }
}