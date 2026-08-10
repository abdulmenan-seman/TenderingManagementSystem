namespace TmsApi.Application.Tenders.Commands.AddTenderDocument;

using MediatR;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Domain.Entities;

public record AddTenderDocumentCommand(UploadTenderDocumentRequestDto DocumentDto) : IRequest<Result<int>>;

public class AddTenderDocumentCommandHandler : IRequestHandler<AddTenderDocumentCommand, Result<int>>
{
    private readonly ITmsDbContext _context;

    public AddTenderDocumentCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(AddTenderDocumentCommand request, CancellationToken cancellationToken)
    {
        var dto = request.DocumentDto;

        var tender = await _context.Tenders.FindAsync(new object[] { dto.TenderId }, cancellationToken);
        if (tender is null)
        {
            return Result<int>.Failure($"Tender with ID {dto.TenderId} not found.");
        }

        try
        {
            var document = new TenderDocument(dto.TenderId, dto.FileName, dto.FilePath);

            _context.TenderDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(document.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
    }
}