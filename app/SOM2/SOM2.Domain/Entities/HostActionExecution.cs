using SOM2.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Domain.Entities
{
    public class HostActionExecution
    {
        public Guid Id { get; set; }

        // Powiązanie z hostem
        public Guid HostId { get; set; }
        public ManagedHost Host { get; set; } = null!;

        // Jaka akcja
        public HostActionType Action { get; set; }

        // Status wykonania
        public HostActionStatus Status { get; set; } = HostActionStatus.Pending;

        // Czas
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;      // kliknięcie
        public DateTime? StartedAt { get; set; }     // start procesu
        public DateTime? FinishedAt { get; set; }    // exit

        // Wynik
        public int? ExitCode { get; set; }
        public string? Output { get; set; }           // skrócony stdout/stderr
    }
}
