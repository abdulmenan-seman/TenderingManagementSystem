namespace TmsApi.Application.Tenders.Handlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.QueriesAndCommands;

public class PublishTenderCommandHandler : IRequestHandler<PublishTenderCommand, Result<bool>>
{
    private readonly ITmsDbContext _context;

    public PublishTenderCommandHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(PublishTenderCommand request, CancellationToken cancellationToken)
    {
        var tender = await _context.Tenders
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (tender is null)
            return Result<bool>.Failure($"Tender with ID {request.Id} was not found.");

        try
        {
            tender.Publish();
            await _context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
