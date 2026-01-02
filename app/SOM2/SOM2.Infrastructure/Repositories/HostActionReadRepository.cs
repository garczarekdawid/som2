using SOM2.Domain.Entities;
using SOM2.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SOM2.Domain.Entities;
using SOM2.Domain.Interfaces;
using SOM2.Infrastructure.Persistence;

namespace SOM2.Infrastructure.Repositories
{
    public class HostActionReadRepository : IHostActionReadRepository
    {
        private readonly AppDbContext _db;

        public HostActionReadRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<HostActionExecution>> GetLatestAsync(int limit = 100)
        {
            return await _db.HostActionExecutions
                .AsNoTracking()
                .Include(x => x.ManagedHost)
                .OrderByDescending(x => x.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<HostActionExecution?> GetByIdAsync(Guid id)
        {
            return await _db.HostActionExecutions
                .AsNoTracking()
                .Include(x => x.ManagedHost)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
