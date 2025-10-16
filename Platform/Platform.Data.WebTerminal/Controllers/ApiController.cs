using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Platform.Data.Triplets;
using Platform.Data.WebTerminal.Models;

namespace Platform.Data.WebTerminal.Controllers
{
    /// <summary>
    /// REST API Controller for Links operations
    /// </summary>
    [ApiController]
    [Route("api/v1")]
    [Produces("application/json")]
    public class ApiController : ControllerBase
    {
        /// <summary>
        /// Get links matching the given restrictions
        /// </summary>
        /// <param name="source">Source (beginning of link, subject) restriction</param>
        /// <param name="linker">Linker (type of connection, verb, predicate, action, operator, transition) restriction</param>
        /// <param name="target">Target (end of link, object) restriction</param>
        /// <returns>An array of links matching the restrictions</returns>
        /// <response code="200">Returns the array of links</response>
        /// <response code="400">If the request parameters are invalid</response>
        [HttpGet("links")]
        [ProducesResponseType(typeof(IEnumerable<LinkDto>), 200)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public IActionResult GetLinks(
            [FromQuery] long? source = null,
            [FromQuery] long? linker = null,
            [FromQuery] long? target = null)
        {
            try
            {
                var results = new List<LinkDto>();

                // If all parameters are null, return all links
                if (!source.HasValue && !linker.HasValue && !target.HasValue)
                {
                    Link.WalkThroughAllLinks(link =>
                    {
                        results.Add(new LinkDto
                        {
                            Id = link.ToInt(),
                            Source = link.Source.ToInt(),
                            Linker = link.Linker.ToInt(),
                            Target = link.Target.ToInt()
                        });
                        return true;
                    });
                }
                else
                {
                    // Build search query based on provided restrictions
                    var sourceLink = source.HasValue ? Link.Restore(source.Value) : Link.Restore(0);
                    var linkerLink = linker.HasValue ? Link.Restore(linker.Value) : Link.Restore(0);
                    var targetLink = target.HasValue ? Link.Restore(target.Value) : Link.Restore(0);

                    // Use Link.Search to find matching link
                    var searchResult = Link.Search(sourceLink, linkerLink, targetLink);
                    if (searchResult != null && searchResult.ToInt() != 0)
                    {
                        results.Add(new LinkDto
                        {
                            Id = searchResult.ToInt(),
                            Source = searchResult.Source.ToInt(),
                            Linker = searchResult.Linker.ToInt(),
                            Target = searchResult.Target.ToInt()
                        });
                    }
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorDto
                {
                    Code = 400,
                    Message = ex.Message,
                    Fields = string.Empty
                });
            }
        }

        /// <summary>
        /// Create a new link
        /// </summary>
        /// <param name="linkDto">The link to create</param>
        /// <returns>The created link</returns>
        /// <response code="201">Returns the newly created link</response>
        /// <response code="400">If the link data is invalid</response>
        [HttpPost("links")]
        [ProducesResponseType(typeof(LinkDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        public IActionResult CreateLink([FromBody] LinkDto linkDto)
        {
            try
            {
                if (linkDto == null)
                {
                    return BadRequest(new ErrorDto
                    {
                        Code = 400,
                        Message = "Link data is required",
                        Fields = "linkDto"
                    });
                }

                var source = Link.Restore(linkDto.Source);
                var linker = Link.Restore(linkDto.Linker);
                var target = Link.Restore(linkDto.Target);

                var newLink = Link.Create(source, linker, target);

                var result = new LinkDto
                {
                    Id = newLink.ToInt(),
                    Source = newLink.Source.ToInt(),
                    Linker = newLink.Linker.ToInt(),
                    Target = newLink.Target.ToInt()
                };

                return CreatedAtAction(nameof(GetLink), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorDto
                {
                    Code = 400,
                    Message = ex.Message,
                    Fields = string.Empty
                });
            }
        }

        /// <summary>
        /// Get a specific link by ID
        /// </summary>
        /// <param name="id">The link ID</param>
        /// <returns>The link</returns>
        /// <response code="200">Returns the link</response>
        /// <response code="404">If the link is not found</response>
        [HttpGet("links/{id}")]
        [ProducesResponseType(typeof(LinkDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        public IActionResult GetLink(long id)
        {
            try
            {
                var link = Link.Restore(id);

                if (link == null || link.ToInt() == 0)
                {
                    return NotFound(new ErrorDto
                    {
                        Code = 404,
                        Message = $"Link with ID {id} not found",
                        Fields = "id"
                    });
                }

                var result = new LinkDto
                {
                    Id = link.ToInt(),
                    Source = link.Source.ToInt(),
                    Linker = link.Linker.ToInt(),
                    Target = link.Target.ToInt()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorDto
                {
                    Code = 400,
                    Message = ex.Message,
                    Fields = string.Empty
                });
            }
        }

        /// <summary>
        /// Update an existing link
        /// </summary>
        /// <param name="id">The link ID to update</param>
        /// <param name="linkDto">The updated link data</param>
        /// <returns>The updated link</returns>
        /// <response code="200">Returns the updated link</response>
        /// <response code="404">If the link is not found</response>
        [HttpPut("links/{id}")]
        [ProducesResponseType(typeof(LinkDto), 200)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        public IActionResult UpdateLink(long id, [FromBody] LinkDto linkDto)
        {
            try
            {
                var link = Link.Restore(id);

                if (link == null || link.ToInt() == 0)
                {
                    return NotFound(new ErrorDto
                    {
                        Code = 404,
                        Message = $"Link with ID {id} not found",
                        Fields = "id"
                    });
                }

                var source = Link.Restore(linkDto.Source);
                var linker = Link.Restore(linkDto.Linker);
                var target = Link.Restore(linkDto.Target);

                Link.Update(ref link, source, linker, target);

                var result = new LinkDto
                {
                    Id = link.ToInt(),
                    Source = link.Source.ToInt(),
                    Linker = link.Linker.ToInt(),
                    Target = link.Target.ToInt()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorDto
                {
                    Code = 400,
                    Message = ex.Message,
                    Fields = string.Empty
                });
            }
        }

        /// <summary>
        /// Delete a link
        /// </summary>
        /// <param name="id">The link ID to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the link was successfully deleted</response>
        /// <response code="404">If the link is not found</response>
        [HttpDelete("links/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ErrorDto), 404)]
        public IActionResult DeleteLink(long id)
        {
            try
            {
                var link = Link.Restore(id);

                if (link == null || link.ToInt() == 0)
                {
                    return NotFound(new ErrorDto
                    {
                        Code = 404,
                        Message = $"Link with ID {id} not found",
                        Fields = "id"
                    });
                }

                Link.Delete(ref link);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorDto
                {
                    Code = 400,
                    Message = ex.Message,
                    Fields = string.Empty
                });
            }
        }
    }
}
