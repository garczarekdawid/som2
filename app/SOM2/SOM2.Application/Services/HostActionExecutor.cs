using SOM2.Application.Common;
using SOM2.Application.Interfaces;
using SOM2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Application.Services
{
    public class HostActionExecutor : IHostActionExecutor
    {
        public async Task<ExecutionResult> ExecuteAsync(HostActionExecution action)
        {
            // Symulacja: losowy czas wykonania 1–3s
            await Task.Delay(new Random().Next(1000, 3000));

            bool success = new Random().NextDouble() > 0.1; // 90% sukces

            return new ExecutionResult
            {
                ExitCode = success ? 0 : 1,
                Output = success ? "Action completed successfully" : "Action failed"
            };
        }
    }
}
