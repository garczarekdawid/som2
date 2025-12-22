using System.ComponentModel.DataAnnotations;

namespace SOM2.Web.Models.ManagedHost
{
    public class ManagedHostEditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string SshUser { get; set; }

        [DataType(DataType.Password)]
        public string? SshPassword { get; set; } // puste = zachowaj stare

        [Required]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$")]
        public string IpAddress { get; set; }

        [Required]
        [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$")]
        public string MacAddress { get; set; }
        public string? Description { get; set; }
    }
}
