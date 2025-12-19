using SOM2.Application.DTO;
using SOM2.Application.Interfaces;
using SOM2.Domain.Entities;
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

        public ManagedHostService(IManagedHostRepository repo)
        {
            _repo = repo;
        }

        //public async Task<List<ManagedHostDto>> GetAllAsync()
        //    => await _repo.GetAllAsync();

        public async Task<List<ManagedHostDto>> GetAllAsync()
        {
            var hosts = await _repo.GetAllAsync(); // List<ManagedHost>

            // Konwersja encji -> DTO
            var dtoList = hosts.Select(h => new ManagedHostDto
            {
                Id = h.Id,
                Name = h.Name,
                IpAddress = h.IpAddress,
                MacAddress = h.MacAddress,
                Description = h.Description
            }).ToList();

            return dtoList;
        }

        public async Task AddAsync(ManagedHostCreateDto dto)
        {
            // Konwersja DTO -> encja domenowa
            var host = dto.ToEntity();
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
                Description = host.Description
            };
        }
    }
}
