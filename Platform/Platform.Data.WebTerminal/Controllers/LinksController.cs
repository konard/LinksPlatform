using Microsoft.AspNetCore.Mvc;
using Platform.Data.Triplets;
using Platform.Data.WebTerminal.Models;

namespace Platform.Data.WebTerminal.Controllers
{
    public class LinksController : Controller
    {
        // GET: /Links/
        public IActionResult Index(long id = 0)
        {
            if (id == 0)
            {
                id = Net.Link;
            }
            var link = Link.Restore(id);
            var model = LinkModel.CreateLinkModel(link);
            return View("Index", model);
        }

        public IActionResult Infinite(long id = 0)
        {
            if (id == 0)
            {
                id = Net.Link;
            }
            var link = Link.Restore(id);
            var model = LinkModel.CreateLinkModel(link);
            return View("Infinite", model);
        }

        public IActionResult Canvas(long id = 0)
        {
            if (id == 0)
            {
                id = Net.Link;
            }
            var link = Link.Restore(id);
            var model = LinkModel.CreateLinkModel(link);
            return View("Canvas", model);
        }

        [HttpPost]
        public IActionResult Create(long source, long target, long? linker = null)
        {
            try
            {
                var sourceLink = Link.Restore(source);
                var targetLink = Link.Restore(target);
                var linkerLink = linker.HasValue ? Link.Restore(linker.Value) : Net.And;

                var link = Link.Create(sourceLink, linkerLink, targetLink);
                return Json(new { success = true, id = link.ToInt(), source = source, target = target, linker = linkerLink.ToInt() });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}