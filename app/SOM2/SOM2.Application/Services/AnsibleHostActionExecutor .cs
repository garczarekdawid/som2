using SOM2.Application.Common;
using SOM2.Application.Interfaces;
using SOM2.Domain.Entities;
using SOM2.Domain.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SOM2.Application.Services
{
    public class AnsibleHostActionExecutor : IHostActionExecutor
    {
        private readonly AnsibleOptions _options;
        private static readonly TimeSpan ExecutionTimeout = TimeSpan.FromSeconds(120);

        public AnsibleHostActionExecutor(AnsibleOptions options)
        {
            _options = options;
        }

        public async Task<ExecutionResult> ExecuteAsync(
            HostActionExecution action,
            CancellationToken ct)
        {
            if (action.Action == HostActionType.PowerOn)
            {
                return await ExecuteWakeOnLanAsync(action, ct);
            }

            return await ExecuteSshActionAsync(action, ct);
        }

        // =========================
        // SSH ACTIONS (PowerOff / Reboot)
        // =========================
        private async Task<ExecutionResult> ExecuteSshActionAsync(
            HostActionExecution action,
            CancellationToken ct)
        {
            var result = new ExecutionResult();
            var host = action.ManagedHost
                ?? throw new InvalidOperationException("Host cannot be null");

            string playbook = action.Action switch
            {
                HostActionType.Reboot => "restart.yml",
                HostActionType.PowerOff => "poweroff.yml",
                _ => throw new NotSupportedException($"Action {action.Action} not supported via SSH")
            };

            string basePath = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, _options.BasePath));

            string playbookPath = Path.Combine(basePath, "Ansible", "playbooks", playbook);
            string inventoryPath = Path.Combine(basePath, "Ansible", "inventory", $"tmp_{action.Id}.ini");

            string legacyArgs = host.LegacySshSupported
                ? " ansible_ssh_common_args='-oHostKeyAlgorithms=+ssh-dss'"
                : "";

            string inventoryContent =
$@"[all]
{host.IpAddress} ansible_user={host.SshUser} ansible_ssh_pass={host.SshPassword}{legacyArgs} ansible_become=true ansible_become_pass={host.SshPassword}";

            await File.WriteAllTextAsync(inventoryPath, inventoryContent, ct);

            string playbookArg = _options.Mode == "Wsl" ? ToWslPath(playbookPath) : playbookPath;
            string inventoryArg = _options.Mode == "Wsl" ? ToWslPath(inventoryPath) : inventoryPath;

            string args = $"-i \"{inventoryArg}\" \"{playbookArg}\"";

            var psi = BuildProcess(args);

            try
            {
                return await RunProcessAsync(psi, ct);
            }
            finally
            {
                try { if (File.Exists(inventoryPath)) File.Delete(inventoryPath); } catch { }
            }
        }

        // =========================
        // WAKE ON LAN (PowerOn)
        // =========================
        private async Task<ExecutionResult> ExecuteWakeOnLanAsync(
            HostActionExecution action,
            CancellationToken ct)
        {
            var host = action.ManagedHost
                ?? throw new InvalidOperationException("Host cannot be null");

            string basePath = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, _options.BasePath));

            string playbookPath = Path.Combine(basePath, "Ansible", "playbooks", "wol.yml");

            string playbookArg = _options.Mode == "Wsl"
                ? ToWslPath(playbookPath)
                : playbookPath;

            string args = $"-i localhost, \"{playbookArg}\" --extra-vars \"mac={host.MacAddress}\"";

            var psi = BuildProcess(args);

            return await RunProcessAsync(psi, ct);
        }

        // =========================
        // PROCESS RUNNER (shared)
        // =========================
        private async Task<ExecutionResult> RunProcessAsync(
            ProcessStartInfo psi,
            CancellationToken ct)
        {
            var result = new ExecutionResult();

            Process? process = null;

            try
            {
                process = Process.Start(psi);
                result.ProcessStarted = process != null;

                if (process == null)
                {
                    result.ExitCode = -1;
                    result.StdErr = "Failed to start ansible process";
                    return result;
                }

                var stdout = new StringBuilder();
                var stderr = new StringBuilder();

                process.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null) stdout.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null) stderr.AppendLine(e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                using var timeoutCts = new CancellationTokenSource(ExecutionTimeout);
                using var linkedCts =
                    CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

                try
                {
                    await process.WaitForExitAsync(linkedCts.Token);
                    result.ExitCode = process.ExitCode;
                }
                catch (OperationCanceledException)
                {
                    try
                    {
                        if (!process.HasExited)
                            process.Kill(entireProcessTree: true);
                    }
                    catch { }

                    result.ExitCode = -1;
                    result.StdErr = timeoutCts.IsCancellationRequested
                        ? "Execution timeout exceeded"
                        : "Execution cancelled";
                }

                result.StdOut = stdout.ToString();
                result.StdErr = stderr.ToString();

                return result;
            }
            catch (Exception ex)
            {
                result.ExitCode = -1;
                result.StdErr = ex.ToString();
                return result;
            }
            finally
            {
                try { process?.Dispose(); } catch { }
            }
        }

        // =========================
        // PROCESS FACTORY
        // =========================
        private ProcessStartInfo BuildProcess(string args)
        {
            var psi = _options.Mode switch
            {
                "Wsl" => new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"ansible-playbook {args}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Environment =
                    {
                        ["LANG"] = "pl_PL.UTF-8",
                        ["LC_ALL"] = "pl_PL.UTF-8",
                        ["ANSIBLE_USE_SSHPASS"] = "true"
                    }
                },
                "Native" => new ProcessStartInfo
                {
                    FileName = "ansible-playbook",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                _ => throw new NotSupportedException($"Mode {_options.Mode} not supported")
            };

            return psi;
        }

        private static string ToWslPath(string windowsPath)
        {
            var fullPath = Path.GetFullPath(windowsPath);
            var drive = char.ToLower(fullPath[0]);
            var path = fullPath.Substring(2).Replace('\\', '/');
            return $"/mnt/{drive}{path}";
        }
    }
}
