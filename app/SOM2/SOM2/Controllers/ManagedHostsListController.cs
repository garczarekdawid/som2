using Microsoft.AspNetCore.Mvc;
using SOM2.Application.DTO;
using SOM2.Application.Interfaces;
using SOM2.Domain.Entities;
using SOM2.Domain.Enums;
using SOM2.Web.Models;

namespace SOM2.Web.Controllers
{
    public class ManagedHostsListController : Controller
    {
        private readonly IManagedHostService _service;

        public ManagedHostsListController(IManagedHostService service)
        {
            _service = service;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            var hosts = await _service.GetAllAsync();
            return View(hosts);
        }

        // =========================
        // SINGLE HOST ACTIONS
        // =========================

        /// <summary>
        /// Wake on LAN
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TurnOn(Guid id)
        {
            return await EnqueueSingleAction(id, HostActionType.PowerOn, "Wake on LAN");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restart(Guid id)
        {
            return await EnqueueSingleAction(id, HostActionType.Reboot, "Restart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TurnOff(Guid id)
        {
            return await EnqueueSingleAction(id, HostActionType.PowerOff, "Wyłączenie");
        }

        // =========================
        // BATCH ACTION
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchAction(
            List<Guid> SelectedIds,
            string Action)
        {
            if (SelectedIds == null || !SelectedIds.Any())
            {
                TempData["Message"] = "Nie wybrano żadnych hostów";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(Action))
            {
                TempData["Message"] = "Nie wybrano akcji";
                return RedirectToAction(nameof(Index));
            }

            HostActionType actionEnum;

            switch (Action)
            {
                case HostActionStrings.TurnOn:
                    actionEnum = HostActionType.PowerOn;   // WOL
                    break;

                case HostActionStrings.TurnOff:
                    actionEnum = HostActionType.PowerOff;
                    break;

                case HostActionStrings.Restart:
                    actionEnum = HostActionType.Reboot;
                    break;

                default:
                    TempData["Message"] = $"Nieznana akcja: {Action}";
                    return RedirectToAction(nameof(Index));
            }

            int queued = 0;

            foreach (var hostId in SelectedIds)
            {
                try
                {
                    await _service.EnqueueActionAsync(hostId, actionEnum);
                    queued++;
                }
                catch (InvalidOperationException)
                {
                    // host ma już akcję w toku – ignorujemy
                }
            }

            TempData["Message"] = $"Akcja '{Action}' dodana dla {queued} hostów";
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // GLOBAL ACTIONS
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TurnOnAll()
        {
            await EnqueueForAllAsync(HostActionType.PowerOn, "Wake on LAN");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TurnOffAll()
        {
            await EnqueueForAllAsync(HostActionType.PowerOff, "Wyłączenie");
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // PRIVATE HELPERS
        // =========================

        private async Task<IActionResult> EnqueueSingleAction(
            Guid hostId,
            HostActionType actionType,
            string actionName)
        {
            try
            {
                var executionId = await _service.EnqueueActionAsync(hostId, actionType);
                TempData["Message"] = $"{actionName} zaplanowane (ActionId: {executionId})";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Message"] = $"Nie można wykonać akcji: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task EnqueueForAllAsync(
            HostActionType actionType,
            string actionName)
        {
            var hosts = await _service.GetAllAsync();
            int queued = 0;

            foreach (var host in hosts)
            {
                try
                {
                    await _service.EnqueueActionAsync(host.Id, actionType);
                    queued++;
                }
                catch (InvalidOperationException)
                {
                    // pomijamy hosty zajęte
                }
            }

            TempData["Message"] = $"{actionName} zaplanowane dla {queued} hostów";
        }
    }
}
