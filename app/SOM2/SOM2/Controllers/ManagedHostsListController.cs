using Microsoft.AspNetCore.Mvc;
using SOM2.Application.DTO;
using SOM2.Application.Interfaces;
using SOM2.Domain.Entities;
using SOM2.Web.Models.ManagedHost;

namespace SOM2.Web.Controllers
{
    public class ManagedHostsListController : Controller
    {
        private readonly IManagedHostService _service;

        public ManagedHostsListController(IManagedHostService service)
        {
            _service = service;
        }

        // Index – lista hostów z akcjami
        public async Task<IActionResult> Index()
        {
            var hosts = await _service.GetAllAsync(); // na razie pobieramy wszystkie

            return View(hosts); // przekazujemy listę DTO
        }

        [HttpPost]
        public async Task<IActionResult> TurnOn(Guid id)
        {
            try
            {
                var executionId = await _service.EnqueueActionAsync(id, HostActionType.PowerOn);
                TempData["Message"] = $"Włączono hosta: {id} (akcja id: {executionId})";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Message"] = $"Nie można wykonać akcji: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restart(Guid id)
        {
            try
            {
                var executionId = await _service.EnqueueActionAsync(id, HostActionType.Reboot);
                TempData["Message"] = $"Zrestartowano hosta: {id} (akcja id: {executionId})";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Message"] = $"Nie można wykonać akcji: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> TurnOff(Guid id)
        {
            try
            {
                var executionId = await _service.EnqueueActionAsync(id, HostActionType.PowerOff);
                TempData["Message"] = $"Wyłączono hosta: {id} (akcja id: {executionId})";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Message"] = $"Nie można wykonać akcji: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> BatchAction(List<Guid> SelectedIds, string Action)
        {
            if (SelectedIds == null || !SelectedIds.Any() || string.IsNullOrEmpty(Action))
                return RedirectToAction("Index");

            var actionEnum = Action switch
            {
                "TurnOn" => HostActionType.PowerOn,
                "TurnOff" => HostActionType.PowerOff,
                "Restart" => HostActionType.Reboot,
                _ => throw new ArgumentException("Nieznana akcja")
            };

            foreach (var hostId in SelectedIds)
            {
                try
                {
                    await _service.EnqueueActionAsync(hostId, actionEnum);
                }
                catch (InvalidOperationException)
                {
                    // Możesz logować lub ignorować hosty z już trwającą akcją
                }
            }

            TempData["Message"] = $"Akcje '{Action}' zapisane dla zaznaczonych hostów";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TurnOnAll()
        {
            var hosts = await _service.GetAllAsync();

            foreach (var host in hosts)
            {
                try
                {
                    await _service.EnqueueActionAsync(host.Id, HostActionType.PowerOn);
                }
                catch (InvalidOperationException)
                {
                    // host już ma akcję w trakcie
                }
            }

            TempData["Message"] = "Wszystkie hosty włączone (enqueue)";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TurnOffAll()
        {
            var hosts = await _service.GetAllAsync();

            foreach (var host in hosts)
            {
                try
                {
                    await _service.EnqueueActionAsync(host.Id, HostActionType.PowerOff);
                }
                catch (InvalidOperationException)
                {
                    // host już ma akcję w trakcie
                }
            }

            TempData["Message"] = "Wszystkie hosty wyłączone (enqueue)";
            return RedirectToAction("Index");
        }
    }
}
