using SOM2.Application.Interfaces;
using SOM2.Application.Common;
using SOM2.Domain.Entities;
using SOM2.Domain.Enums;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace SOM2.Application.Services
{
    public class AnsibleHostActionExecutor : IHostActionExecutor
    {
        private readonly AnsibleOptions _options;

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
                _ => throw new NotSupportedException($"Akcja {action.Action} nie jest obsługiwana")
            };

            // Ścieżka do katalogu Ansible
            string basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, _options.BasePath));
            string playbookPath = Path.Combine(basePath, "Ansible\\playbooks", playbook);

            // Zachowujemy ścieżkę Windows dla czyszczenia
            string inventoryWindowsPath = Path.Combine(basePath, "Ansible\\inventory", $"tmp_{action.Id}.ini");

            // Legacy SSH
            string legacyArgs = host.LegacySshSupported
                ? " ansible_ssh_common_args='-oHostKeyAlgorithms=+ssh-dss'"
                : "";

            // Dynamiczne inventory - Linux/WSL kompatybilne
            string inventoryContent = $"[all]\n{host.IpAddress} ansible_user={host.SshUser} ansible_ssh_pass={host.SshPassword}{legacyArgs} ansible_become=true ansible_become_pass={host.SshPassword}";
            await File.WriteAllTextAsync(inventoryWindowsPath, inventoryContent, ct);

            // Przygotowanie ścieżek do argumentów
            string playbookPathForArgs = playbookPath;
            string inventoryPathForArgs = inventoryWindowsPath;

            if (_options.Mode == "Wsl")
            {
                playbookPathForArgs = ToWslPath(playbookPath);
                inventoryPathForArgs = ToWslPath(inventoryWindowsPath);
            }

            // Argumenty do ansible-playbook
            string args = $"-i \"{inventoryPathForArgs}\" \"{playbookPathForArgs}\"";

            // Konfiguracja procesu
            ProcessStartInfo psi = _options.Mode switch
            {
                "Wsl" => new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"ansible-playbook {args}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                "Native" => new ProcessStartInfo
                {
                    FileName = "ansible-playbook",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                _ => throw new NotSupportedException($"Tryb {_options.Mode} nieobsługiwany")
            };

            // WSL: wymuszamy użycie sshpass
            if (_options.Mode == "Wsl")
            {
                psi.Environment["ANSIBLE_USE_SSHPASS"] = "true";
            }

            try
            {
                using var process = Process.Start(psi);
                result.ProcessStarted = process != null;

                if (process == null)
                    return result;

                var stdout = new StringBuilder();
                var stderr = new StringBuilder();

                process.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
                process.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(ct);

                result.ExitCode = process.ExitCode;
                result.StdOut = stdout.ToString();
                result.StdErr = stderr.ToString();
            }
            catch (Exception ex)
            {
                result.StdErr = ex.ToString();
            }
            finally
            {
                // Usuń tymczasowe inventory (Windows path!)
                if (File.Exists(inventoryWindowsPath))
                {
                    try { File.Delete(inventoryWindowsPath); } catch { /* ignorujemy */ }
                }
            }

            return result;
        }

        // Zamiana ścieżki Windows -> WSL
        private static string ToWslPath(string windowsPath)
        {
            var fullPath = Path.GetFullPath(windowsPath);
            var drive = char.ToLower(fullPath[0]);
            var path = fullPath.Substring(2).Replace('\\', '/');
            return $"/mnt/{drive}{path}";
        }
    }
}
