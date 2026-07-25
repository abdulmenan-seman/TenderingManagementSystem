namespace TmsApi.Application.Bids.Commands.AddBidDocument;

using MediatR;
using TmsApi.Application.Bids.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Domain.Entities;

public record AddBidDocumentCommand(AddBidDocumentRequestDto DocumentDto) : IRequest<Result<int>>;

public class AddBidDocumentCommandHandler : IRequestHandler<AddBidDocumentCommand, Result<int>>
{
    private readonly ITmsDbContext _context;

    public AddBidDocumentCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(AddBidDocumentCommand request, CancellationToken cancellationToken)
    {
        var dto = request.DocumentDto;

        var bid = await _context.Bids.FindAsync(new object[] { dto.BidId }, cancellationToken);
        if (bid is null)
        {
            return Result<int>.Failure($"Bid with ID {dto.BidId} was not found.");
        }

        try
        {
            var document = new BidDocument(
                dto.BidId,
                dto.DocumentType,
                dto.FileName,
                dto.FilePath
            );

            _context.BidDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(document.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
    }
}