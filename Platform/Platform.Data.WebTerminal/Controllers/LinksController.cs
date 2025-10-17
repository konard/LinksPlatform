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
        public IActionResult Create(long source, long target)
        {
            try
            {
                var link = Link.Create(source, target);
                return Json(new { success = true, id = link.ToInt(), source = source, target = target });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}