using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SOM2.Application.Interfaces;
using SOM2.Domain.Entities;
using SOM2.Domain.Enums;
using SOM2.Domain.Interfaces;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SOM2.Application.Common;

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

                var pending = await repo.GetPendingActionsAsync(limit: 10);

                foreach (var action in pending)
                {
                    try
                    {
                        action.Status = HostActionStatus.Running;
                        action.StartedAt = DateTime.UtcNow;
                        await repo.UpdateAsync(action);

                        var result = await executor.ExecuteAsync(action, stoppingToken);

                        action.ExitCode = result.ExitCode;
                        action.Output = CleanString(result.StdOut) + "\n" + CleanString(result.StdErr);
                        action.Status = result.ExitCode == 0
                            ? HostActionStatus.Success
                            : HostActionStatus.Failed;
                    }
                    catch (Exception ex)
                    {
                        action.Status = HostActionStatus.Failed;
                        action.Output = $"Execution threw exception: {ex.Message}";
                    }

                    action.FinishedAt = DateTime.UtcNow;
                    await repo.UpdateAsync(action);
                }

                await Task.Delay(5000, stoppingToken);
            }
        }


        private async Task ExecuteSingleAsync(
            HostActionExecution action,
            IHostActionRepository repo,
            IHostActionExecutor executor,
            CancellationToken stoppingToken)
        {
            try
            {
                var result = await executor.ExecuteAsync(action, stoppingToken);

                action.ExitCode = result.ExitCode;
                action.Output = CleanString(result.StdOut) + "\n" + CleanString(result.StdErr);
                action.Status = result.ExitCode == 0 ? HostActionStatus.Success : HostActionStatus.Failed;
            }
            catch (Exception ex)
            {
                action.Status = HostActionStatus.Failed;
                action.Output = $"Execution threw exception: {ex.Message}";
            }

            action.FinishedAt = DateTime.UtcNow;
            await repo.UpdateAsync(action);
        }

        string CleanString(string input)
        {
            return input?.Replace("\0", "") ?? "";
        }

    }
}
