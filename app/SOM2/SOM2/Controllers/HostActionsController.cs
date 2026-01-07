using Microsoft.AspNetCore.Mvc;
using SOM2.Application.Interfaces;
using SOM2.Domain.Enums;
using SOM2.Domain.Interfaces;

namespace SOM2.Web.Controllers
{
    public class HostActionsController : Controller
    {
        private readonly IHostActionQueryService _queryService;
        private readonly IHostActionRepository _repo;

        public HostActionsController(IHostActionQueryService queryService, IHostActionRepository repo)
        {
            _queryService = queryService;
            _repo = repo;
        }

        // GET: /HostActions
        public async Task<IActionResult> Index()
        {
            var list = await _queryService.GetLatestAsync(100);
            return View(list);
        }

        // GET: /HostActions/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var action = await _queryService.GetByIdAsync(id);
            if (action == null)
                return NotFound();

            return View(action);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var action = await _repo.GetByIdAsync(id);
            if (action == null)
                return NotFound();

            if (action.Status == SOM2.Domain.Enums.HostActionStatus.Pending ||
                action.Status == SOM2.Domain.Enums.HostActionStatus.Running)
            {
                action.Status = SOM2.Domain.Enums.HostActionStatus.Cancelled;
                action.FinishedAt = DateTime.UtcNow;
                action.Output += "\n[Action cancelled by admin]";
                await _repo.UpdateAsync(action);
            }

            TempData["Message"] = $"Akcja {id} została zakończona administracyjnie.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
