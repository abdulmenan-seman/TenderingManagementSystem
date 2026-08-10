namespace TmsApi.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Bids.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

public class BidService : IBidService
{
    private readonly TmsDbContext _context;

    public BidService(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<BidResponseDto> SubmitBidAsync(SubmitBidRequestDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validate Tender existence
        var tenderExists = await _context.Tenders
            .AnyAsync(t => t.Id == dto.TenderId && !t.IsDeleted, cancellationToken);
        if (!tenderExists)
        {
            throw new KeyNotFoundException($"Tender with ID {dto.TenderId} was not found.");
        }

        // 2. Validate Supplier existence
        var supplierExists = await _context.Users
            .AnyAsync(u => u.Id == dto.SupplierId && !u.IsDeleted, cancellationToken);
        if (!supplierExists)
        {
            throw new KeyNotFoundException($"Supplier with ID {dto.SupplierId} was not found.");
        }

        // 3. Ensure supplier has not already submitted a bid for this tender
        var existingBid = await _context.Bids
            .AnyAsync(b => b.TenderId == dto.TenderId && b.SupplierId == dto.SupplierId && !b.IsDeleted, cancellationToken);
        if (existingBid)
        {
            throw new InvalidOperationException($"Supplier ID {dto.SupplierId} has already submitted a bid for Tender ID {dto.TenderId}.");
        }

        // 4. Create entity via domain constructor
        var bid = new Bid(dto.TenderId, dto.SupplierId, dto.FinancialProposalAmount);

        _context.Bids.Add(bid);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(bid);
    }

    public async Task<BidDocumentDto> AddBidDocumentAsync(AddBidDocumentRequestDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Verify Bid exists
        var bid = await _context.Bids
            .FirstOrDefaultAsync(b => b.Id == dto.BidId && !b.IsDeleted, cancellationToken);
        if (bid == null)
        {
            throw new KeyNotFoundException($"Bid with ID {dto.BidId} was not found.");
        }

        // 2. Instantiate BidDocument via domain constructor
        var document = new BidDocument(dto.BidId, dto.DocumentType, dto.FileName, dto.FilePath);

        _context.BidDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDocumentDto(document);
    }

    public async Task<BidResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var bid = await _context.Bids
            .Include(b => b.Documents)
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);

        return bid == null ? null : MapToDto(bid);
    }

    public async Task<IEnumerable<BidResponseDto>> GetBidsByTenderIdAsync(int tenderId, CancellationToken cancellationToken = default)
    {
        var bids = await _context.Bids
            .Include(b => b.Documents)
            .Where(b => b.TenderId == tenderId && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        return bids.Select(MapToDto);
    }

    public async Task<IEnumerable<BidResponseDto>> GetBidsBySupplierIdAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        var bids = await _context.Bids
            .Include(b => b.Documents)
            .Where(b => b.SupplierId == supplierId && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        return bids.Select(MapToDto);
    }

    public async Task<bool> UpdateBidStatusAsync(int id, BidStatus status, CancellationToken cancellationToken = default)
    {
        var bid = await _context.Bids.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted, cancellationToken);
        if (bid == null) return false;

        // Domain method mutation
        bid.UpdateStatus(status);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static BidResponseDto MapToDto(Bid bid)
    {
        var documents = bid.Documents?
            .Select(MapToDocumentDto)
            .ToList() ?? new List<BidDocumentDto>();

        return new BidResponseDto(
            bid.Id,
            bid.TenderId,
            bid.SupplierId,
            bid.FinancialProposalAmount,
            bid.SubmissionDate,
            bid.Status.ToString(),
            documents
        );
    }

    private static BidDocumentDto MapToDocumentDto(BidDocument doc)
    {
        return new BidDocumentDto(
            doc.Id,
            doc.DocumentType,
            doc.FileName,
            doc.FilePath,
            doc.UploadedAt
        );
    }
}