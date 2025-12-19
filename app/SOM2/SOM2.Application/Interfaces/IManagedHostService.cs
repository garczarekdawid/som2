using SOM2.Application.DTO;
using SOM2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Application.Interfaces
{
    public interface IManagedHostService
    {
        Task<List<ManagedHostDto>> GetAllAsync();
        Task AddAsync(ManagedHostCreateDto host);
        Task DeleteAsync(Guid id);
        Task UpdateAsync(ManagedHostUpdateDto host);

        Task<ManagedHostUpdateDto?> GetByIdAsync(Guid id);
    }
}
