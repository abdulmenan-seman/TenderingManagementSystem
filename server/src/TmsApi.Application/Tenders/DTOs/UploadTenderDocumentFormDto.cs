namespace TmsApi.Application.Tenders.DTOs;

using System.IO;

// Form DTO accepting stream and file name
public record UploadTenderDocumentFormDto(
    Stream File,
    string FileName
);