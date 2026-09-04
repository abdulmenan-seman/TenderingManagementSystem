namespace TmsApi.Application.Common.Interfaces;

using System.IO;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folderName, CancellationToken cancellationToken = default);
}