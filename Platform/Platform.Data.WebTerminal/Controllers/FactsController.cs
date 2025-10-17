using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Platform.Data.Triplets;
using Platform.Data.WebTerminal.Models;

namespace Platform.Data.WebTerminal.Controllers
{
    public class FactsController : Controller
    {
        private static readonly Dictionary<long, FactModel> _factsStorage = new Dictionary<long, FactModel>();
        private static long _nextId = 1;

        // GET: /Facts/
        public IActionResult Index(bool showDrafts = false)
        {
            var facts = _factsStorage.Values.AsEnumerable();

            // Filter out drafts for non-authenticated users
            if (!showDrafts && !User.Identity.IsAuthenticated)
            {
                facts = facts.Where(f => !f.IsDraft);
            }

            return View(facts.ToList());
        }

        // GET: /Facts/Details/5
        public IActionResult Details(long id)
        {
            if (!_factsStorage.TryGetValue(id, out var fact))
            {
                return NotFound();
            }

            // Check if it's a draft and user is not authenticated
            if (fact.IsDraft && !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            return View(fact);
        }

        // GET: /Facts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Facts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FactModel fact)
        {
            if (ModelState.IsValid)
            {
                fact.Id = _nextId++;
                _factsStorage[fact.Id] = fact;
                return RedirectToAction(nameof(Details), new { id = fact.Id });
            }
            return View(fact);
        }

        // GET: /Facts/Edit/5
        public IActionResult Edit(long id)
        {
            if (!_factsStorage.TryGetValue(id, out var fact))
            {
                return NotFound();
            }
            return View(fact);
        }

        // POST: /Facts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(long id, FactModel fact)
        {
            if (id != fact.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _factsStorage[id] = fact;
                return RedirectToAction(nameof(Details), new { id = fact.Id });
            }
            return View(fact);
        }

        // GET: /Facts/Delete/5
        public IActionResult Delete(long id)
        {
            if (!_factsStorage.TryGetValue(id, out var fact))
            {
                return NotFound();
            }
            return View(fact);
        }

        // POST: /Facts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(long id)
        {
            _factsStorage.Remove(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
