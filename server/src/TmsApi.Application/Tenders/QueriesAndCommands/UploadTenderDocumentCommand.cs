namespace TmsApi.Application.Tenders.QueriesAndCommands;

using MediatR;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;

public record UploadTenderDocumentCommand(
    int TenderId, 
    string FileName, 
    byte[] Content,
    string ContentType
) : IRequest<Result<TenderDocumentDto>>;