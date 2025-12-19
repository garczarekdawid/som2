namespace SOM2.Web.Models.ManagedHost
{
    public class ManagedHostEditViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SshUser { get; set; }
        public string? SshPassword { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string? Description { get; set; }
    }
}
