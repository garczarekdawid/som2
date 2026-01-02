using SOM2.Application.DTO;
using SOM2.Application.Interfaces;
using SOM2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Application.Services
{
    public class HostActionQueryService : IHostActionQueryService
    {
        private readonly IHostActionReadRepository _repo;

        public HostActionQueryService(IHostActionReadRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<HostActionListDto>> GetLatestAsync(int limit = 100)
        {
            var actions = await _repo.GetLatestAsync(limit);
            return actions.Select(x => new HostActionListDto
            {
                Id = x.Id,
                HostName = x.ManagedHost.Name,
                Action = x.Action,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                StartedAt = x.StartedAt,
                FinishedAt = x.FinishedAt
            }).ToList();
        }

        public async Task<HostActionDetailsDto?> GetByIdAsync(Guid id)
        {
            var x = await _repo.GetByIdAsync(id);
            if (x == null) return null;

            return new HostActionDetailsDto
            {
                Id = x.Id,
                HostName = x.ManagedHost.Name,
                Action = x.Action,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                StartedAt = x.StartedAt,
                FinishedAt = x.FinishedAt,
                ExitCode = x.ExitCode,
                Output = x.Output,
                //Error = x.Error
            };
        }
    }
}
