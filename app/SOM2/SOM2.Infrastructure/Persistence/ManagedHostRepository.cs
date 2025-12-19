using Microsoft.EntityFrameworkCore;
using SOM2.Domain.Entities;
using SOM2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Infrastructure.Persistence
{
    public class ManagedHostRepository : IManagedHostRepository
    {
        private readonly AppDbContext _db;

        public ManagedHostRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ManagedHost>> GetAllAsync()
        => await _db.ManagedHosts.ToListAsync();

        public async Task<ManagedHost?> GetByIdAsync(Guid id)
            => await _db.ManagedHosts.FindAsync(id);

        public async Task AddAsync(ManagedHost host)
        {
            _db.ManagedHosts.Add(host);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(ManagedHost host)
        {
            _db.ManagedHosts.Update(host);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(ManagedHost host)
        {
            _db.ManagedHosts.Remove(host);
            await _db.SaveChangesAsync();
        }
    }
}
