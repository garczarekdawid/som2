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

        public async Task<ExecutionResult> ExecuteAsync(HostActionExecution action, CancellationToken ct)
        {
            var result = new ExecutionResult();
            var host = action.ManagedHost ?? throw new InvalidOperationException("Host cannot be null");

            // Wybór playbooka
            string playbook = action.Action switch
            {
                HostActionType.Reboot => "restart.yml",
                HostActionType.PowerOff => "poweroff.yml",
                _ => throw new NotSupportedException($"Action {action.Action} not supported")
            };

            // Ścieżki
            string basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _options.BasePath));
            string playbookWindowsPath = Path.Combine(basePath, "Ansible", "playbooks", playbook);
            string inventoryWindowsPath = Path.Combine(basePath, "Ansible", "inventory", $"tmp_{action.Id}.ini");

            // Legacy SSH
            string legacyArgs = host.LegacySshSupported
                ? " ansible_ssh_common_args='-oHostKeyAlgorithms=+ssh-dss'"
                : "";

            // Dynamiczne inventory
            string inventoryContent =
$@"[all]
{host.IpAddress} ansible_user={host.SshUser} ansible_ssh_pass={host.SshPassword}{legacyArgs} ansible_become=true ansible_become_pass={host.SshPassword}";

            await File.WriteAllTextAsync(inventoryWindowsPath, inventoryContent, ct);

            // Ścieżki dla argów
            string playbookArgPath = _options.Mode == "Wsl" ? ToWslPath(playbookWindowsPath) : playbookWindowsPath;
            string inventoryArgPath = _options.Mode == "Wsl" ? ToWslPath(inventoryWindowsPath) : inventoryWindowsPath;
            string args = $"-i \"{inventoryArgPath}\" \"{playbookArgPath}\"";

            // Konfiguracja procesu
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
                        ["LC_ALL"] = "pl_PL.UTF-8"
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

            if (_options.Mode == "Wsl")
                psi.Environment["ANSIBLE_USE_SSHPASS"] = "true";

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

                process.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
                process.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Timeout + administracyjny cancel
                using var timeoutCts = new CancellationTokenSource(ExecutionTimeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

                try
                {
                    await process.WaitForExitAsync(linkedCts.Token);
                    result.ExitCode = process.ExitCode;
                }
                catch (OperationCanceledException)
                {
                    try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { }
                    result.ExitCode = -1;
                    result.StdErr = timeoutCts.IsCancellationRequested
                        ? "Execution timeout exceeded"
                        : "Execution cancelled by user";
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
                try { if (File.Exists(inventoryWindowsPath)) File.Delete(inventoryWindowsPath); } catch { }
                try { process?.Dispose(); } catch { }
            }
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
