using System;
using System.ComponentModel.DataAnnotations;

namespace PhotoLibApi.Models
{
    /// <summary>
    /// Represents metadata for a single photo stored in the system.
    /// The actual image file is stored separately (e.g., in Blob storage),
    /// while this model holds descriptive information.
    /// </summary>
    public class Photo
    {
        /// <summary>
        /// Permanent unique identifier generated on the server.
        /// This is the main reference used by the API and the database.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Temporary identifier created on the client before the photo
        /// is synchronized with the server. Used to map local entries
        /// to server-side records during sync.
        /// </summary>
        public string? ClientTempId { get; set; }

        /// <summary>
        /// Human-readable title of the photo.
        /// </summary>
        [Required]
        public string Title { get; set; } = "";

        /// <summary>
        /// Optional text description of the photo.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Identifier of the gallery to which this photo belongs.
        /// </summary>
        [Required]
        public Guid GalleryId { get; set; }

        /// <summary>
        /// URL of the original image file stored externally.
        /// </summary>
        public string? BlobUrl { get; set; }

        /// <summary>
        /// URL of the thumbnail image for quick display in lists.
        /// </summary>
        public string? ThumbnailUrl { get; set; }

        /// <summary>
        /// Optional metadata in JSON form (e.g., EXIF information).
        /// </summary>
        public string? ExifJson { get; set; }

        /// <summary>
        /// Marks the photo as deleted without removing it from the database.
        /// Useful for synchronization and recovery.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when this record was created (in UTC).
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp of the latest update (in UTC).
        /// Used to detect changes during synchronization.
        /// </summary>
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Concurrency token used by Entity Framework to detect conflicts
        /// when multiple updates happen at the same time.
        /// </summary>
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
