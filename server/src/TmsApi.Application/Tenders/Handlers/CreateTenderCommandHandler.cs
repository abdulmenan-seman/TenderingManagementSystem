namespace TmsApi.Application.Tenders.Handlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Application.Tenders.QueriesAndCommands;
using TmsApi.Domain.Entities;

public class CreateTenderCommandHandler : IRequestHandler<CreateTenderCommand, Result<TenderResponseDto>>
{
    private readonly ITmsDbContext _context;

    public CreateTenderCommandHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<TenderResponseDto>> Handle(CreateTenderCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // 1. Database Check (Verify Reference Number Uniqueness)
        var exists = await _context.Tenders
            .AnyAsync(t => t.ReferenceNumber == dto.ReferenceNumber, cancellationToken);

        if (exists)
            return Result<TenderResponseDto>.Failure($"Reference number '{dto.ReferenceNumber}' is already in use.");

        try
        {
            // 2. Instantiate Aggregate Root (Enforces Domain Rules)
            var tender = new Tender(
                dto.ReferenceNumber,
                dto.Title,
                dto.Description,
                dto.EstimatedBudget,
                dto.SubmissionDeadline,
                request.OfficerId
            );

            // 3. Attach Initial Documents if included
            if (dto.InitialDocuments != null)
            {
                foreach (var doc in dto.InitialDocuments)
                {
                    tender.AddDocument(doc.FileName, doc.FilePath);
                }
            }

            // 4. Save Aggregate Root to Database
            _context.Tenders.Add(tender);
            await _context.SaveChangesAsync(cancellationToken);

            // 5. Build DTO Response
            var documentDtos = tender.Documents
                .Select(d => new TenderDocumentDto(d.Id, d.FileName, d.FilePath, d.UploadedAt))
                .ToList();

            var response = new TenderResponseDto(
                tender.Id,
                tender.ReferenceNumber,
                tender.Title,
                tender.Description,
                tender.EstimatedBudget,
                tender.PublicationDate,
                tender.SubmissionDeadline,
                tender.Status.ToString(),
                tender.CreatedByOfficerId,
                documentDtos
            );

            return Result<TenderResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<TenderResponseDto>.Failure(ex.Message);
        }
    }
}