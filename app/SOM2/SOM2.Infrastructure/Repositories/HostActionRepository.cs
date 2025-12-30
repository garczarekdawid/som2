using Microsoft.EntityFrameworkCore;
using SOM2.Domain.Entities;
using SOM2.Domain.Enums;
using SOM2.Domain.Interfaces;
using SOM2.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Infrastructure.Repositories
{
    public class HostActionRepository : IHostActionRepository
    {
        private readonly AppDbContext _db;

        public HostActionRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(HostActionExecution execution)
        {
            _db.HostActionExecutions.Add(execution);
            await _db.SaveChangesAsync();
        }

        public async Task<HostActionExecution?> GetByIdAsync(Guid id)
        {
            return await _db.HostActionExecutions
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<HostActionExecution?> GetNextPendingAsync()
        {
            return await _db.HostActionExecutions
                .Where(x => x.Status == HostActionStatus.Pending)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(HostActionExecution execution)
        {
            _db.HostActionExecutions.Update(execution);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> HasRunningActionAsync(Guid hostId)
        {
            return await _db.HostActionExecutions
                .AnyAsync(x =>
                    x.HostId == hostId &&
                    x.Status == HostActionStatus.Running);
        }

        public async Task<List<HostActionExecution>> GetPendingActionsAsync()
        {
            return await _db.HostActionExecutions
                .Where(x => x.Status == HostActionStatus.Pending)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }


        public async Task<HostActionExecution?> GetLastForHostAsync(Guid hostId)
        {
            return await _db.HostActionExecutions
                .Where(x => x.HostId == hostId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
