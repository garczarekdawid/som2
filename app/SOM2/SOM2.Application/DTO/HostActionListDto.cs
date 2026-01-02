using SOM2.Domain.Entities;
using SOM2.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Application.DTO
{
    public class HostActionListDto
    {
        public Guid Id { get; set; }
        public string HostName { get; set; }
        public HostActionType Action { get; set; }
        public HostActionStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
