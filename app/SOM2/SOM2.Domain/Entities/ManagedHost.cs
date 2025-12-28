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
        public string? Description { get; private set; }
        public bool LegacySshSupported { get; private set; } = false;

        public void SetStatus(HostStatus status)
        {
            Status = status;
        }

        public void SetName(string name) => Name = name;
        public void SetSshUser(string sshUser) => SshUser = sshUser;
        public void SetSshPassword(string sshPassword) => SshPassword = sshPassword;
        public void SetIpAddress(string ip) => IpAddress = ip;
        public void SetMacAddress(string mac) => MacAddress = mac;
        public void SetDescription(string description) => Description = description;

        public void SetLegacySshSupported(bool value) => LegacySshSupported = value;

        public ManagedHost(string name, string sshUser, string sshPassword, string ipAddress, string macAddress, string description = "", bool legacySshSupported = false)
        {
            Id = Guid.NewGuid();
            Name = name;
            SshUser = sshUser;
            SshPassword = sshPassword;
            IpAddress = ipAddress;
            MacAddress = macAddress;
            Description = description;
            Status = HostStatus.Unknown;
            LegacySshSupported = legacySshSupported;
        }

        public ManagedHost(Guid id, string name, string sshUser, string sshPassword, string ipAddress, string macAddress, string description = "", bool legacySshSupported = false)
        {
            Id = id;
            Name = name;
            SshUser = sshUser;
            SshPassword = sshPassword;
            IpAddress = ipAddress;
            MacAddress = macAddress;
            Description = description;
            Status = HostStatus.Unknown;
            LegacySshSupported = legacySshSupported;
        }
    }
}
