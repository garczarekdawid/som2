using SOM2.Application.Common;
using SOM2.Application.DTO;
using SOM2.Application.Interfaces;
using SOM2.Domain.Entities;
using SOM2.Domain.Enums;
using SOM2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SOM2.Application.Services
{
    public class ManagedHostService : IManagedHostService
    {
        private readonly IManagedHostRepository _repo;
        private readonly IHostActionRepository _hostActionRepo;

        public ManagedHostService(IManagedHostRepository repo, IHostActionRepository hostActionRepo)
        {
            _repo = repo;
            _hostActionRepo = hostActionRepo;
        }

        //public async Task<List<ManagedHostDto>> GetAllAsync()
        //    => await _repo.GetAllAsync();

        public async Task<List<ManagedHostDto>> GetAllAsync()
        {
            var hosts = await _repo.GetAllAsync(); // List<ManagedHost>

            var dtoList = new List<ManagedHostDto>();

            foreach (var h in hosts)
            {
                var lastAction = await _hostActionRepo.GetLastForHostAsync(h.Id); // metoda repo do napisania

                dtoList.Add(new ManagedHostDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    IpAddress = h.IpAddress,
                    MacAddress = h.MacAddress,
                    Description = h.Description,
                    LegacySshSupported = h.LegacySshSupported,
                    LastActionType = lastAction?.Action,
                    LastActionStatus = lastAction?.Status,
                    LastActionTime = lastAction?.CreatedAt,
                    LastActionId = lastAction?.Id
                });
            }

            return dtoList;
        }

        public async Task AddAsync(ManagedHostCreateDto dto)
        {
            // Konwersja DTO -> encja domenowa
            var host = dto.ToEntity();
            host.SetLegacySshSupported(dto.LegacySshSupported);
            await _repo.AddAsync(host);
        }

        public async Task DeleteAsync(Guid id)
        {
            var host = await _repo.GetByIdAsync(id);
            if (host == null) return;

            await _repo.DeleteAsync(host);
        }

        public async Task UpdateAsync(ManagedHostUpdateDto dto)
        {
            var host = await _repo.GetByIdAsync(dto.Id);
            if (host == null) throw new Exception("Host not found");

            host.SetName(dto.Name ?? host.Name);
            host.SetSshUser(dto.SshUser ?? host.SshUser);
            host.SetSshPassword(string.IsNullOrEmpty(dto.SshPassword) ? host.SshPassword : dto.SshPassword);
            host.SetIpAddress(dto.IpAddress ?? host.IpAddress);
            host.SetMacAddress(dto.MacAddress ?? host.MacAddress);
            host.SetDescription(dto.Description ?? host.Description);
            host.SetLegacySshSupported(dto.LegacySshSupported);

            await _repo.UpdateAsync(host);
        }

        public async Task<ManagedHostUpdateDto?> GetByIdAsync(Guid id)
        {
            var host = await _repo.GetByIdAsync(id); // zwraca encję
            if (host == null) return null;

            return new ManagedHostUpdateDto
            {
                Id = host.Id,
                Name = host.Name,
                SshUser = host.SshUser,
                SshPassword = host.SshPassword,
                IpAddress = host.IpAddress,
                MacAddress = host.MacAddress,
                Description = host.Description,
                LegacySshSupported = host.LegacySshSupported
            };
        }

        public async Task<(IEnumerable<ManagedHostDto> Hosts, int TotalCount)> GetPagedAsync(PaginationParams pagination, ManagedHostFilter? filter = null)
        {
            var (hosts, totalCount) = await _repo.GetPagedAsync(pagination.Page, pagination.PageSize, filter?.Search);

            var dtoList = hosts.Select(h => new ManagedHostDto
            {
                Id = h.Id,
                Name = h.Name,
                IpAddress = h.IpAddress,
                MacAddress = h.MacAddress,
                Description = h.Description,
                LegacySshSupported = h.LegacySshSupported
            });

            return (dtoList, totalCount);
        }

        public async Task<List<ManagedHostUpdateDto>> GetByIdsAsync(List<Guid> ids)
        {
            var list = new List<ManagedHostUpdateDto>();

            foreach (var id in ids)
            {
                var host = await GetByIdAsync(id);
                if (host != null)
                    list.Add(host);
            }

            return list;
        }


        public async Task<Guid> EnqueueActionAsync(Guid hostId, HostActionType action)
        {
            // blokada jednoczesnych akcji na tym samym hoście
            if (await _hostActionRepo.HasRunningActionAsync(hostId))
                throw new InvalidOperationException("Host ma już działającą akcję");

            var execution = new HostActionExecution
            {
                Id = Guid.NewGuid(),
                HostId = hostId,
                Action = action,
                Status = HostActionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _hostActionRepo.AddAsync(execution);

            return execution.Id;
        }

        public async Task<bool> HasRunningActionAsync(Guid hostId)
            => await _hostActionRepo.HasRunningActionAsync(hostId);
    }
}
