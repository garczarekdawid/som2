namespace SOM2.Web.Models.ManagedHost
{
    public class ManagedHostIndexViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string? Description { get; set; }
    }
}
