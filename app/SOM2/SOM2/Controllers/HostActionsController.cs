using Microsoft.AspNetCore.Mvc;
using SOM2.Application.Interfaces;

namespace SOM2.Web.Controllers
{
    public class HostActionsController : Controller
    {
        private readonly IHostActionQueryService _queryService;

        public HostActionsController(IHostActionQueryService queryService)
        {
            _queryService = queryService;
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
    }
}
