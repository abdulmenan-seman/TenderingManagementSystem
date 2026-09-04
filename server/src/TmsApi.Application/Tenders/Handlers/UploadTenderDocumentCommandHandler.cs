namespace TmsApi.Application.Tenders.Handlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Application.Tenders.QueriesAndCommands;
using TmsApi.Domain.Entities;

public class UploadTenderDocumentCommandHandler : IRequestHandler<UploadTenderDocumentCommand, Result<TenderDocumentDto>>
{
    private readonly ITmsDbContext _context;

    public UploadTenderDocumentCommandHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<TenderDocumentDto>> Handle(UploadTenderDocumentCommand request, CancellationToken cancellationToken)
    {
        var tender = await _context.Tenders
            .Include(t => t.Documents)
            .FirstOrDefaultAsync(t => t.Id == request.TenderId && !t.IsDeleted, cancellationToken);

        if (tender is null)
            return Result<TenderDocumentDto>.Failure($"Tender with ID {request.TenderId} was not found.");

        try
        {
            // Use domain method to add the document entity
            tender.AddDocument(request.FileName, request.Content, request.ContentType);
            
            await _context.SaveChangesAsync(cancellationToken);

            // Get the newly created document
            var createdDocument = tender.Documents.Last();

            var dto = new TenderDocumentDto(
                createdDocument.Id,
                createdDocument.FileName,
                $"/api/v1/tenders/documents/{createdDocument.Id}/download",
                createdDocument.UploadedAt
            );

            return Result<TenderDocumentDto>.Success(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Result<TenderDocumentDto>.Failure(ex.Message);
        }
    }
}