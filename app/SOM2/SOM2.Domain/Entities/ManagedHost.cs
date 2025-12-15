using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Domain.Entities
{
    public class ManagedHost
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string SshUser { get; private set; }
        public string SshPassword { get; private set; }
        public string IpAddress { get; private set; }
        public string MacAddress { get; private set; }
        public HostStatus Status { get; private set; }
        public string Description { get; private set; }

        public void SetStatus(HostStatus status)
        {
            Status = status;
        }
    }
}
