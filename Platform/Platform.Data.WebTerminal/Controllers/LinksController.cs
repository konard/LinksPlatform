using Microsoft.AspNetCore.Mvc;
using Platform.Data.Triplets;
using Platform.Data.WebTerminal.Models;
using System;
using System.Collections.Generic;

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

        // REST API Endpoints

        /// <summary>
        /// Get a link by ID
        /// </summary>
        /// <param name="id">Link ID</param>
        /// <returns>Link information</returns>
        [HttpGet("api/links/{id}")]
        public ActionResult<object> GetLink(long id)
        {
            try
            {
                var link = Link.Restore(id);
                return Ok(new
                {
                    id = link.ToInt(),
                    source = link.Source != null ? link.Source.ToInt() : (long?)null,
                    linker = link.Linker != null ? link.Linker.ToInt() : (long?)null,
                    target = link.Target != null ? link.Target.ToInt() : (long?)null,
                    totalReferers = link.TotalReferers,
                    timestamp = link.Timestamp
                });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new link
        /// </summary>
        /// <param name="request">Link creation request</param>
        /// <returns>Created link information</returns>
        [HttpPost("api/links")]
        public ActionResult<object> CreateLink([FromBody] CreateLinkRequest request)
        {
            try
            {
                var sourceLink = Link.Restore(request.Source);
                var linkerLink = Link.Restore(request.Linker);
                var targetLink = Link.Restore(request.Target);
                var newLink = Link.Create(sourceLink, linkerLink, targetLink);

                return CreatedAtAction(nameof(GetLink), new { id = newLink.ToInt() }, new
                {
                    id = newLink.ToInt(),
                    source = newLink.Source != null ? newLink.Source.ToInt() : (long?)null,
                    linker = newLink.Linker != null ? newLink.Linker.ToInt() : (long?)null,
                    target = newLink.Target != null ? newLink.Target.ToInt() : (long?)null,
                    totalReferers = newLink.TotalReferers,
                    timestamp = newLink.Timestamp
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Search for links by criteria
        /// </summary>
        /// <param name="source">Optional source link ID</param>
        /// <param name="linker">Optional linker link ID</param>
        /// <param name="target">Optional target link ID</param>
        /// <returns>List of matching links</returns>
        [HttpGet("api/links")]
        public ActionResult<object> SearchLinks([FromQuery] long? source = null, [FromQuery] long? linker = null, [FromQuery] long? target = null)
        {
            try
            {
                var results = new List<object>();

                if (source.HasValue)
                {
                    var sourceLink = Link.Restore(source.Value);
                    sourceLink.WalkThroughReferers(referer =>
                    {
                        var matchesLinker = !linker.HasValue || (referer.Linker != null && referer.Linker.ToInt() == linker.Value);
                        var matchesTarget = !target.HasValue || (referer.Target != null && referer.Target.ToInt() == target.Value);

                        if (matchesLinker && matchesTarget)
                        {
                            results.Add(new
                            {
                                id = referer.ToInt(),
                                source = referer.Source != null ? referer.Source.ToInt() : (long?)null,
                                linker = referer.Linker != null ? referer.Linker.ToInt() : (long?)null,
                                target = referer.Target != null ? referer.Target.ToInt() : (long?)null,
                                totalReferers = referer.TotalReferers,
                                timestamp = referer.Timestamp
                            });
                        }
                    });
                }

                return Ok(new { count = results.Count, links = results });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a link by ID
        /// </summary>
        /// <param name="id">Link ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("api/links/{id}")]
        public ActionResult DeleteLink(long id)
        {
            try
            {
                var link = Link.Restore(id);
                Link.Delete(link);
                return Ok(new { message = "Link deleted successfully", id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get statistics about the link database
        /// </summary>
        /// <returns>Database statistics</returns>
        [HttpGet("api/stats")]
        public ActionResult<object> GetStats()
        {
            try
            {
                // Platform.Data.Triplets doesn't expose Total/AllocatedLinks in older versions
                // Return basic info instead
                return Ok(new
                {
                    status = "operational",
                    message = "Links database is running"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class CreateLinkRequest
    {
        public long Source { get; set; }
        public long Linker { get; set; }
        public long Target { get; set; }
    }
}