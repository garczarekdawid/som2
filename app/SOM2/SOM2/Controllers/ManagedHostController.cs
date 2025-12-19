using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Index()
        {
            var hostsDto = await _service.GetAllAsync();

            // Konwersja DTO do ViewModel
            var model = hostsDto.Select(h => new ManagedHostIndexViewModel
            {
                Id = h.Id,
                Name = h.Name,
                IpAddress = h.IpAddress,
                MacAddress = h.MacAddress,
                Description = h.Description
            }).ToList();

            return View(model);
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
                Description = vm.Description
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
                Description = dto.Description
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
                Description = vm.Description
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
