namespace TmsApi.Application.Tenders.Commands.CreateTender;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Domain.Entities;

public record CreateTenderCommand(CreateTenderRequestDto TenderDto) : IRequest<Result<int>>;

public class CreateTenderCommandHandler : IRequestHandler<CreateTenderCommand, Result<int>>
{
    private readonly ITmsDbContext _context;

    public CreateTenderCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateTenderCommand request, CancellationToken cancellationToken)
    {
        var dto = request.TenderDto;

        // Rule Check: Unique reference number
        var exists = await _context.Tenders.AnyAsync(t => t.ReferenceNumber == dto.ReferenceNumber, cancellationToken);
        if (exists)
        {
            return Result<int>.Failure($"Tender reference number '{dto.ReferenceNumber}' is already in use.");
        }

        try
        {
            // Instantiate Aggregate Root (runs Domain Guard Clauses)
            var tender = new Tender(
                dto.ReferenceNumber,
                dto.Title,
                dto.Description,
                dto.EstimatedBudget,
                dto.SubmissionDeadline,
                dto.CreatedByOfficerId
            );

            _context.Tenders.Add(tender);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(tender.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
    }
}