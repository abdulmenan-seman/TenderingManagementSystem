namespace TmsApi.Application.Tenders.QueriesAndCommands;

using MediatR;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;

public record UploadTenderDocumentCommand(
    int TenderId, 
    string FileName, 
    string FilePath
) : IRequest<Result<TenderDocumentDto>>;