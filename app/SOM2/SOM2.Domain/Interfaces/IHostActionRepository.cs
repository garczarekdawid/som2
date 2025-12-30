using SOM2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Domain.Interfaces
{
    public interface IHostActionRepository
    {
        Task AddAsync(HostActionExecution execution);
        Task<HostActionExecution?> GetByIdAsync(Guid id);
        Task<HostActionExecution?> GetNextPendingAsync();
        Task UpdateAsync(HostActionExecution execution);
        Task<bool> HasRunningActionAsync(Guid hostId);

        Task<List<HostActionExecution>> GetPendingActionsAsync();

        Task<HostActionExecution?> GetLastForHostAsync(Guid hostId);
    }
}
