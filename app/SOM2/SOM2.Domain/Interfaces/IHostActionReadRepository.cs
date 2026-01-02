using SOM2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Domain.Interfaces
{
    public interface IHostActionReadRepository
    {
        Task<List<HostActionExecution>> GetLatestAsync(int limit = 100);
        Task<HostActionExecution?> GetByIdAsync(Guid id);
    }
}
