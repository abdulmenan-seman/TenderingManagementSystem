namespace TmsApi.Application.Tenders.Handlers;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.QueriesAndCommands;

public class StartTenderEvaluationCommandHandler : IRequestHandler<StartTenderEvaluationCommand, Result<bool>>
{
    private readonly ITmsDbContext context;

    public StartTenderEvaluationCommandHandler(ITmsDbContext context) => this.context = context;

    public async Task<Result<bool>> Handle(StartTenderEvaluationCommand request, CancellationToken cancellationToken)
    {
        var tender = await context.Tenders.FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);
        if (tender is null) return Result<bool>.Failure($"Tender with ID {request.Id} was not found.");

        try
        {
            tender.StartEvaluation();
            await context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}

public class CloseTenderCommandHandler : IRequestHandler<CloseTenderCommand, Result<bool>>
{
    private readonly ITmsDbContext context;

    public CloseTenderCommandHandler(ITmsDbContext context) => this.context = context;

    public async Task<Result<bool>> Handle(CloseTenderCommand request, CancellationToken cancellationToken)
    {
        var tender = await context.Tenders.FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);
        if (tender is null) return Result<bool>.Failure($"Tender with ID {request.Id} was not found.");

        try
        {
            tender.CloseTender();
            await context.SaveChangesAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}