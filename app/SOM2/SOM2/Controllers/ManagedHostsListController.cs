using Microsoft.AspNetCore.Mvc;
using SOM2.Application.Interfaces;
using SOM2.Application.DTO;
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
        public IActionResult TurnOn(Guid id)
        {
            // Wydmuszka – na razie tylko tekst
            Console.WriteLine($"Włączono hosta: {id}");
            TempData["Message"] = $"Włączono hosta: {id}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Restart(Guid id)
        {
            Console.WriteLine($"Restart hosta: {id}");
            TempData["Message"] = $"Restart hosta: {id}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult TurnOff(Guid id)
        {
            Console.WriteLine($"Wyłączono hosta: {id}");
            TempData["Message"] = $"Wyłączono hosta: {id}";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> BatchAction(List<Guid> SelectedIds, string Action)
        {
            if (SelectedIds == null || !SelectedIds.Any() || string.IsNullOrEmpty(Action))
                return RedirectToAction("Index");

            var hosts = await _service.GetByIdsAsync(SelectedIds);
            var hostInfo = string.Join(", ", hosts.Select(h => $"{h.Name} ({h.IpAddress})"));

            TempData["Message"] = Action switch
            {
                "TurnOn" => $"Włączono hosty: {hostInfo}",
                "TurnOff" => $"Wyłączono hosty: {hostInfo}",
                "Restart" => $"Zrestartowano hosty: {hostInfo}",
                _ => "Akcja wykonana"
            };

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TurnOnAll()
        {
            //await _service.TurnOnAllHostsAsync();
            TempData["Message"] = "Wszystkie hosty włączone";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TurnOffAll()
        {
            //await _service.TurnOffAllHostsAsync();
            TempData["Message"] = "Wszystkie hosty wyłączone";
            return RedirectToAction("Index");
        }
    }
}
