using System;
using Microsoft.AspNetCore.Mvc;
using Platform.Data.Triplets;
using Platform.Data.WebTerminal.Models;

namespace Platform.Data.WebTerminal.Controllers
{
    /// <summary>
    /// Controller for managing references (facts/links) supporting responses
    /// </summary>
    public class ReferencesController : Controller
    {
        // POST: /References/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(long responseId, string title, string url, string description, ReferenceType type)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("Title and URL are required");
            }

            try
            {
                // TODO: Verify reference is not copyrighted content (only facts or links to references)
                // TODO: Store reference in Links storage
                return RedirectToAction("Details", "Responses", new { id = responseId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Unable to create reference: {ex.Message}");
            }
        }

        // POST: /References/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(long id, long responseId)
        {
            try
            {
                // TODO: Remove reference from Links storage
                return RedirectToAction("Details", "Responses", new { id = responseId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Unable to delete reference: {ex.Message}");
            }
        }
    }
}
