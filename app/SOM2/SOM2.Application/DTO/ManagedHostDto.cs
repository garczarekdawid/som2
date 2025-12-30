using SOM2.Domain.Entities;
using SOM2.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM2.Application.DTO
{
    public class ManagedHostCreateDto
    {
        
        public string Name { get; set; }
        public string SshUser { get; set; }
        public string SshPassword { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string? Description { get; set; }
        public bool LegacySshSupported { get; set; }
    }

    public static class ManagedHostCreateDtoExtensions
    {
        public static ManagedHost ToEntity(this ManagedHostCreateDto dto)
        {
            return new ManagedHost(
                dto.Name ?? "",
                dto.SshUser ?? "",
                dto.SshPassword ?? "",
                dto.IpAddress ?? "",
                dto.MacAddress ?? "",
                dto.Description ?? "",
                dto.LegacySshSupported
            );
        }
    }


    public class ManagedHostDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string Description { get; set; }

        public bool LegacySshSupported { get; set; }

        public HostActionType? LastActionType { get; set; }
        public HostActionStatus? LastActionStatus { get; set; }
        public DateTime? LastActionTime { get; set; }
    }

    public class ManagedHostUpdateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SshUser { get; set; }
        public string SshPassword { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string? Description { get; set; }

        public bool LegacySshSupported { get; set; }


 
    }


    public static class ManagedHostUpdateDtooExtensions
    {
        public static ManagedHost ToEntity(this ManagedHostUpdateDto dto)
        {
            return new ManagedHost(
                dto.Id,
                dto.Name ?? "",
                dto.SshUser ?? "",
                dto.SshPassword ?? "",
                dto.IpAddress ?? "",
                dto.MacAddress ?? "",
                dto.Description ?? "",
                dto.LegacySshSupported
            );
        }
    }

}
