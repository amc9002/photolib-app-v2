using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoLibApi.Data;
using PhotoLibApi.Models;

namespace PhotoLibApi.Controllers
{
    /// <summary>
    /// Controller to manage user galleries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class GalleryController : ControllerBase
    {
        private readonly PhotoDbContext _db;

        public GalleryController(PhotoDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns the list of galleries for the current user.
        /// </summary>
        /// <remarks>
        /// Uses the current authenticated user if available.
        /// For local testing (no auth), it returns galleries where OwnerId is null.
        /// </remarks>
        /// <response code="200">A list of galleries belonging to the user.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Gallery>>> GetAll()
        {
            // simple owner resolution: use authenticated user name or null for local testing
            var ownerId = User?.Identity?.Name;

            var q = _db.Galleries.AsNoTracking().Where(g => g.OwnerId == ownerId);
            var list = await q.OrderBy(g => g.CreatedAtUtc).ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// Creates a new gallery.
        /// </summary>
        ///  <remarks>
        /// Currently, the gallery is created without authentication.
        /// OwnerId will be null until authentication is added.
        /// </remarks>
        /// <response code="201">Gallery created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Gallery>> Create(
            [FromBody] CreateGalleryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var gallery = new Gallery
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                OwnerId = User?.Identity?.Name,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Galleries.Add(gallery);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), gallery);
        }

        /// <summary>
        /// Deletes a gallery by its identifier.
        /// </summary>
        /// <param name="id">Gallery identifier.</param>
        /// <response code="204">Gallery deleted successfully.</response>
        /// <response code="404">Gallery not found.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var gallery = await _db.Galleries.FindAsync(id);

            if (gallery == null)
                return NotFound();

            _db.Galleries.Remove(gallery);
            await _db.SaveChangesAsync();

            return NoContent();
        }

    }
}
