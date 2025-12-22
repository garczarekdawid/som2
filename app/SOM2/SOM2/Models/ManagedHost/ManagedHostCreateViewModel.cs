using System.ComponentModel.DataAnnotations;

namespace SOM2.Web.Models.ManagedHost
{
    public class ManagedHostCreateViewModel
    {
        [Required(ErrorMessage = "Nazwa hosta jest wymagana")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Użytkownik SSH jest wymagany")]
        public string SshUser { get; set; }

        [Required(ErrorMessage = "Hasło SSH jest wymagane")]
        [DataType(DataType.Password)]
        public string SshPassword { get; set; }

        [Required(ErrorMessage = "IP jest wymagane")]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$",
        ErrorMessage = "Niepoprawny format IP")]
        public string IpAddress { get; set; }

        [Required(ErrorMessage = "MAC jest wymagany")]
        [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
        ErrorMessage = "Niepoprawny format MAC")]
        public string MacAddress { get; set; }

        public string? Description { get; set; }
    }
}

