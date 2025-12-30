using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SOM2.Domain.Entities;
using SOM2.Domain.Interfaces;
using SOM2.Domain.Enums;
using SOM2.Application.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SOM2.Infrastructure.Workers
{
    public class HostActionWorker : BackgroundService
    {
        private readonly IServiceProvider _services;

        public HostActionWorker(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IHostActionRepository>();
                    var executor = scope.ServiceProvider.GetRequiredService<IHostActionExecutor>();

                    var pending = await repo.GetPendingActionsAsync();

                    foreach (var action in pending)
                    {
                        action.Status = HostActionStatus.Running;
                        await repo.UpdateAsync(action);

                        try
                        {
                            bool success = await executor.ExecuteAsync(action);

                            action.Status = success ? HostActionStatus.Success : HostActionStatus.Failed;
                        }
                        catch
                        {
                            action.Status = HostActionStatus.Failed;
                        }

                        action.FinishedAt = DateTime.UtcNow;
                        await repo.UpdateAsync(action);
                    }
                }

                await Task.Delay(5000, stoppingToken); // 5s między sprawdzeniami
            }
        }
    }
}
