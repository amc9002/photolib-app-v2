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
    }
}
