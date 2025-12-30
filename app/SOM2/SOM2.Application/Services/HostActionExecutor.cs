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
        public async Task<bool> ExecuteAsync(HostActionExecution action)
        {
            // Symulacja: losowy czas wykonania 1–3s
            await Task.Delay(new Random().Next(1000, 3000));

            // Symulacja sukcesu/awarii losowo
            return new Random().NextDouble() > 0.1; // 90% sukces
        }
    }
}
