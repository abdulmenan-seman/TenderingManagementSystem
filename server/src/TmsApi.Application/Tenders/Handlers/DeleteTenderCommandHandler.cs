namespace TmsApi.Application.Tenders.Handlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.QueriesAndCommands;

public class DeleteTenderCommandHandler : IRequestHandler<DeleteTenderCommand, Result<bool>>
{
    private readonly ITmsDbContext _context;

    public DeleteTenderCommandHandler(ITmsDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(DeleteTenderCommand request, CancellationToken cancellationToken)
    {
        var tender = await _context.Tenders
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (tender is null)
            return Result<bool>.Failure($"Tender with ID {request.Id} was not found.");

        if (tender.Status != Domain.Entities.TenderStatus.Draft)
            return Result<bool>.Failure("Only Draft tenders can be deleted.");

        tender.SoftDelete();
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
