using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoLibApi.Data;
using PhotoLibApi.Models;

namespace PhotoLibApi.Controllers
{
    /// <summary>
    /// Controller for working with photos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PhotoController : ControllerBase
    {
        private readonly PhotoDbContext _db;
        private readonly string _photosPath;

        public PhotoController(PhotoDbContext db, IConfiguration configuration)
        {
            _db = db;

            _photosPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                configuration["Storage:PhotosPath"]!);
        }

        /// <summary>
        /// Returns photos belonging to a gallery.
        /// </summary>
        /// <param name="galleryId">Gallery identifier.</param>
        /// <response code="200">List of photos.</response>
        [HttpGet("by-gallery/{galleryId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Photo>>> GetByGallery(Guid galleryId)
        {
            var photos = await _db.Photos
                .AsNoTracking()
                .Where(p => p.GalleryId == galleryId)
                .OrderBy(p => p.CreatedAtUtc)
                .ToListAsync();

            return Ok(photos);
        }

        /// <summary>
        /// Returns the original image file for a photo.
        /// </summary>
        /// <param name="id">Identifier of the photo.</param>
        /// <response code="200">Image file returned.</response>
        /// <response code="404">Photo file not found.</response>
        [HttpGet("{id:guid}/file")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetFile(Guid id)
        {

            // Check that photo exists and has original    
            var photo = _db.Photos.Find(id);
            if (photo == null || !photo.HasOriginal)
                return NotFound();

            // Directory where photos are stored
            var uploadsDir = _photosPath;

            // File path: {photoId}.jpg
            var filePath = Path.Combine(uploadsDir, $"{id}.jpg");

            // Check if file exists
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            // Return file as-is
            return PhysicalFile(
                filePath,
                "image/jpeg");
        }


        /// <summary>
        /// Creates a photo metadata record.
        /// </summary>
        /// <remarks>
        /// This endpoint creates metadata only.
        /// The image file will be uploaded in a later step.
        /// </remarks>
        /// <response code="201">Photo metadata created.</response>
        /// <response code="400">Invalid request data.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Photo>> Create(
            [FromBody] CreatePhotoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1️⃣ Reject empty Guid explicitly
            if (request.GalleryId == Guid.Empty)
                return BadRequest("GalleryId must not be empty.");

            // 2️⃣ Check that the gallery exists
            var galleryExists = await _db.Galleries
                .AsNoTracking()
                .AnyAsync(g => g.Id == request.GalleryId);

            if (!galleryExists)
                return NotFound($"Gallery with id '{request.GalleryId}' not found.");

            var photo = new Photo
            {
                Id = Guid.NewGuid(),
                GalleryId = request.GalleryId,
                Title = request.Title,
                Description = request.Description,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Photos.Add(photo);
            await _db.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, photo);
        }

        /// <summary>
        /// Uploads an original image file for an existing photo.
        /// </summary>
        /// <remarks>
        /// This endpoint accepts a multipart/form-data request and stores
        /// the uploaded image file on the server file system.
        /// The file is saved using the photo identifier as its filename.
        /// </remarks>
        /// <param name="id">Identifier of the photo.</param>
        /// <param name="file">Image file to upload.</param>
        /// <response code="204">File uploaded successfully.</response>
        /// <response code="400">File is missing or empty.</response>
        /// <response code="404">Photo with the specified id was not found.</response>
        [HttpPost("{id:guid}/upload")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Upload(Guid id, IFormFile file)
        {
            // 1️⃣ Validate file
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            // 2️⃣ Check: photo exists in DB
            var photo = await _db.Photos.FindAsync(id);
            if (photo == null)
                return NotFound();

            // 3️⃣ Target directory for uploaded photos
            var uploadsDir = _photosPath;

            // 4️⃣ Create directory if not exists
            Directory.CreateDirectory(uploadsDir);

            // 5️⃣ Full path to the file: {photoId}.jpg 
            var filePath = Path.Combine(uploadsDir, $"{id}.jpg");

            // 6️⃣ Save file to disk
            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);

            // mark that original file exists
            photo.HasOriginal = true;
            await _db.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Updates photo metadata.
        /// </summary>
        /// <param name="id">Photo identifier.</param>
        /// <param name="request"></param>
        /// <response code="204">Photo updated successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="404">Photo not found.</response>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdatePhotoRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var photo = await _db.Photos.FindAsync(id);

            if (photo == null)
                return NotFound();

            photo.Title = request.Title;
            photo.Description = request.Description;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deletes a photo by its identifier.
        /// </summary>
        /// <param name="id">Photo identifier.</param>
        /// <response code="204">Photo deleted successfully.</response>
        /// <response code="404">Photo not found.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var photo = await _db.Photos.FindAsync(id);

            if (photo == null)
                return NotFound();

            _db.Photos.Remove(photo);
            await _db.SaveChangesAsync();

            return NoContent();
        }

    }
}
