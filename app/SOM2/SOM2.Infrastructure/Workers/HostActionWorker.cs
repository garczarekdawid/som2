using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SOM2.Application.Interfaces;
using SOM2.Domain.Entities;
using SOM2.Domain.Enums;
using SOM2.Domain.Interfaces;
using SOM2.Application.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

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
                using var scope = _services.CreateScope();

                var repo = scope.ServiceProvider.GetRequiredService<IHostActionRepository>();
                var executor = scope.ServiceProvider.GetRequiredService<IHostActionExecutor>();

                var pending = await repo.GetPendingActionsAsync();

                foreach (var action in pending)
                {
                    action.Status = HostActionStatus.Running;
                    action.StartedAt = DateTime.UtcNow;
                    await repo.UpdateAsync(action);

                    try
                    {
                        ExecutionResult result = await executor.ExecuteAsync(action, stoppingToken);

                        action.ExitCode = result.ExitCode;
                        action.Output = result.StdOut + "\n" + result.StdErr;

                        // ustawienie statusu w zależności od typu akcji
                        switch (action.Action)
                        {
                            case HostActionType.Reboot:
                                action.Status = result.ExitCode == 0 ? HostActionStatus.Success : HostActionStatus.Failed;
                                break;

                            case HostActionType.PowerOff:
                                // unreachable lub non-zero = OK
                                action.Status = HostActionStatus.Success;
                                break;

                            default:
                                action.Status = HostActionStatus.Failed;
                                break;
                        }
                    }
                    catch
                    {
                        action.Status = HostActionStatus.Failed;
                    }

                    action.FinishedAt = DateTime.UtcNow;
                    await repo.UpdateAsync(action);
                }

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
