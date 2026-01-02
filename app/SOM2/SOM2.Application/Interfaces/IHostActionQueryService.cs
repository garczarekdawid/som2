using SOM2.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Application.Interfaces
{
    public interface IHostActionQueryService
    {
        Task<List<HostActionListDto>> GetLatestAsync(int limit = 100);
        Task<HostActionDetailsDto?> GetByIdAsync(Guid id);
    }
}
