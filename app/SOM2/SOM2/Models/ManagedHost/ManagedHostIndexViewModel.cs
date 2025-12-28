using SOM2.Application.DTO;

namespace SOM2.Web.Models.ManagedHost
{
    public class ManagedHostIndexViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string? Description { get; set; }
        public bool LegacySshSupported { get; set; }

        public IEnumerable<ManagedHostDto> Hosts { get; set; } = new List<ManagedHostDto>();

        // Paginacja
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }

        // Wyszukiwanie / filtry
        public string? Search { get; set; }

        public List<int> PageSizes { get; set; } = new List<int> { 5, 10, 20 };
    }
}
