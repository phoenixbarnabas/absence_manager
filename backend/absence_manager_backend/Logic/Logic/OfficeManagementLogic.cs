using Data;
using Entities.Dtos.LocationDtos;
using Entities.Dtos.OfficeDtos;
using Entities.Dtos.WorkStationDtos;
using Entities.Models;
using Logic.Helper;
using System;
using System.Collections.Generic;
using System.Text;

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

        public IEnumerable<OfficeViewDto> GetOfficesByLocation(int locationId, bool activeOnly = false)
        {
            var query = _officeRepository.GetAll()
                .Where(x => x.LocationId == locationId);

            if (activeOnly)
                query = query.Where(x => x.IsActive);

            return query
                .OrderBy(x => x.DisplayOrder)
                .ToList()
                .Select(x => _dtoProvider.Mapper.Map<OfficeViewDto>(x));
        }

        public IEnumerable<WorkstationViewDto> GetWorkstationsByOffice(int officeId, bool activeOnly = false)
        {
            var query = _workstationRepository.GetAll()
                .Where(x => x.OfficeId == officeId);

            if (activeOnly)
                query = query.Where(x => x.IsActive);

            return query
                .OrderBy(x => x.DisplayOrder)
                .ToList()
                .Select(x => _dtoProvider.Mapper.Map<WorkstationViewDto>(x));
        }

        public WorkstationViewDto GetWorkstationById(int workstationId)
        {
            var workstation = _workstationRepository.FindById(workstationId);
            if (workstation == null)
                throw new KeyNotFoundException("Workstation not found.");

            return _dtoProvider.Mapper.Map<WorkstationViewDto>(workstation);
        }
    }
}
