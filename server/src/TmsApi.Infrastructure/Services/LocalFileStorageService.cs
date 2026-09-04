namespace TmsApi.Infrastructure.Services;

using Microsoft.AspNetCore.Hosting;
using TmsApi.Application.Common.Interfaces;

/// <summary>
/// Persists uploaded files to the local file system under wwwroot/uploads/{folderName}/.
/// Returns a relative URL path that can be stored in the database and served statically.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string folderName,
        CancellationToken cancellationToken = default)
    {
        // Sanitise the original filename and prepend a GUID to avoid collisions
        var safeFileName = Path.GetFileName(fileName); // strips any directory traversal
        var uniqueFileName = $"{Guid.NewGuid():N}_{safeFileName}";

        // Resolve physical path: <contentRoot>/wwwroot/uploads/<folderName>/
        var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", folderName);
        Directory.CreateDirectory(uploadsRoot); // idempotent

        var physicalPath = Path.Combine(uploadsRoot, uniqueFileName);

        await using var destinationStream = new FileStream(
            physicalPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        await fileStream.CopyToAsync(destinationStream, cancellationToken);

        // Return a relative URL path for storage (e.g. /uploads/tenders/abc123_spec.pdf)
        return $"/uploads/{folderName}/{uniqueFileName}";
    }
}
