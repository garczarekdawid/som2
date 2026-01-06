using SOM2.Application.Common;
using SOM2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Application.Interfaces
{
    public interface IHostActionExecutor
    {
        Task<ExecutionResult> ExecuteAsync(HostActionExecution action, CancellationToken ct);
    }
}
