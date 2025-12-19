using SOM2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Domain.Interfaces
{
    public interface IManagedHostRepository
    {
        Task<List<ManagedHost>> GetAllAsync();
        Task<ManagedHost?> GetByIdAsync(Guid id);
        Task AddAsync(ManagedHost host);
        Task UpdateAsync(ManagedHost host);
        Task DeleteAsync(ManagedHost host);
    }
}
