using Microsoft.AspNetCore.Mvc;
using SOM2.Application.Common;
using SOM2.Application.DTO;
using SOM2.Application.Interfaces;
using SOM2.Web.Models;
using SOM2.Web.Models.ManagedHost;

namespace SOM2.Web.Controllers
{
    public class ManagedHostController : Controller
    {
        private readonly IManagedHostService _service;

        public ManagedHostController(IManagedHostService service)
        {
            _service = service;
        }

        // Index – lista hostów
        public async Task<IActionResult> Index(int page = 1, string? search = null, int pageSize = 10)
        {
            // Tworzymy parametry paginacji z dynamiczną ilością wpisów
            var pagination = new PaginationParams
            {
                Page = page,
                PageSize = pageSize
            };

            var filter = new ManagedHostFilter
            {
                Search = search
            };

            // Pobranie danych z serwisu
            var (hosts, totalCount) = await _service.GetPagedAsync(pagination, filter);

            // Przygotowanie ViewModel
            var vm = new ManagedHostIndexViewModel
            {
                Hosts = hosts,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pagination.PageSize),
                PageSize = pagination.PageSize,
                Search = search
            };

            return View(vm);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManagedHostCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Konwersja ViewModel -> DTO
            var dto = new ManagedHostCreateDto
            {
                Name = vm.Name,
                SshUser = vm.SshUser,
                SshPassword = vm.SshPassword,
                IpAddress = vm.IpAddress,
                MacAddress = vm.MacAddress,
                Description = vm.Description,
                LegacySshSupported = vm.LegacySshSupported
            };

            await _service.AddAsync(dto);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            var vm = new ManagedHostEditViewModel
            {
                Id = id,
                Name = dto.Name,
                SshUser = dto.SshUser,
                SshPassword = dto.SshPassword,
                IpAddress = dto.IpAddress,
                MacAddress = dto.MacAddress,
                Description = dto.Description,
                LegacySshSupported = dto.LegacySshSupported
            };

            return View(vm);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ManagedHostEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var dto = new ManagedHostUpdateDto
            {
                Id = vm.Id,
                Name = vm.Name,
                SshUser = vm.SshUser,
                SshPassword = vm.SshPassword,
                IpAddress = vm.IpAddress,
                MacAddress = vm.MacAddress,
                Description = vm.Description,
                LegacySshSupported = vm.LegacySshSupported
            };

            await _service.UpdateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        // POST: Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
