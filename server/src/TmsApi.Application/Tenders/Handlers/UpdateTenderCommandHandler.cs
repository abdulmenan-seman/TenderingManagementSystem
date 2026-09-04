namespace TmsApi.Application.Tenders.Handlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Application.Tenders.QueriesAndCommands;

public class UpdateTenderCommandHandler : IRequestHandler<UpdateTenderCommand, Result<TenderResponseDto>>
{
    private readonly ITmsDbContext _context;

    public UpdateTenderCommandHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<TenderResponseDto>> Handle(UpdateTenderCommand request, CancellationToken cancellationToken)
    {
        var tender = await _context.Tenders
            .Include(t => t.Documents)
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (tender is null)
            return Result<TenderResponseDto>.Failure($"Tender with ID {request.Id} was not found.");

        if (tender.Status != Domain.Entities.TenderStatus.Draft)
            return Result<TenderResponseDto>.Failure("Only Draft tenders can be edited.");

        // Check reference number uniqueness (excluding self)
        var refExists = await _context.Tenders
            .AnyAsync(t => t.ReferenceNumber == request.Dto.ReferenceNumber && t.Id != request.Id, cancellationToken);

        if (refExists)
            return Result<TenderResponseDto>.Failure($"Reference number '{request.Dto.ReferenceNumber}' is already in use.");

        try
        {
            tender.Update(
                request.Dto.ReferenceNumber,
                request.Dto.Title,
                request.Dto.Description,
                request.Dto.EstimatedBudget,
                request.Dto.SubmissionDeadline);

            await _context.SaveChangesAsync(cancellationToken);

            var documentDtos = tender.Documents
                .Select(d => new TenderDocumentDto(d.Id, d.FileName, d.FilePath, d.UploadedAt))
                .ToList();

            return Result<TenderResponseDto>.Success(new TenderResponseDto(
                tender.Id,
                tender.ReferenceNumber,
                tender.Title,
                tender.Description,
                tender.EstimatedBudget,
                tender.PublicationDate,
                tender.SubmissionDeadline,
                tender.Status.ToString(),
                tender.CreatedByOfficerId,
                documentDtos));
        }
        catch (Exception ex)
        {
            return Result<TenderResponseDto>.Failure(ex.Message);
        }
    }
}
