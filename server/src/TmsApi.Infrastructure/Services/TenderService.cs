namespace TmsApi.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Tenders.DTOs;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

public class TenderService : ITenderService
{
    private readonly TmsDbContext _context;

    public TenderService(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<TenderResponseDto> CreateTenderAsync(CreateTenderRequestDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Verify Officer User exists
        var officerExists = await _context.Users
            .AnyAsync(u => u.Id == dto.CreatedByOfficerId && !u.IsDeleted, cancellationToken);
        if (!officerExists)
        {
            throw new KeyNotFoundException($"Officer user with ID {dto.CreatedByOfficerId} was not found.");
        }

        // 2. Instantiate Tender via domain constructor (encapsulates validation & initial Draft status)
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

        return MapToDto(tender);
    }

    public async Task<TenderDocumentDto> UploadTenderDocumentAsync(UploadTenderDocumentRequestDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Verify Tender exists
        var tender = await _context.Tenders
            .FirstOrDefaultAsync(t => t.Id == dto.TenderId && !t.IsDeleted, cancellationToken);
        if (tender == null)
        {
            throw new KeyNotFoundException($"Tender with ID {dto.TenderId} was not found.");
        }

        // 2. Create document via domain constructor
        var document = new TenderDocument(dto.TenderId, dto.FileName, dto.FilePath);

        _context.TenderDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDocumentDto(document);
    }

    public async Task<TenderResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tender = await _context.Tenders
            .Include(t => t.Documents)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);

        return tender == null ? null : MapToDto(tender);
    }

    public async Task<IEnumerable<TenderResponseDto>> GetAllAsync(TenderStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Tenders
            .Include(t => t.Documents)
            .Where(t => !t.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var tenders = await query.ToListAsync(cancellationToken);
        return tenders.Select(MapToDto);
    }

    public async Task<IEnumerable<TenderResponseDto>> GetByOfficerIdAsync(int officerId, CancellationToken cancellationToken = default)
    {
        var tenders = await _context.Tenders
            .Include(t => t.Documents)
            .Where(t => t.CreatedByOfficerId == officerId && !t.IsDeleted)
            .ToListAsync(cancellationToken);

        return tenders.Select(MapToDto);
    }

    public async Task<bool> PublishTenderAsync(int id, CancellationToken cancellationToken = default)
    {
        var tender = await _context.Tenders.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
        if (tender == null) return false;

        // Domain method handles invariant state check (must be Draft)
        tender.Publish();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CloseTenderAsync(int id, CancellationToken cancellationToken = default)
    {
        var tender = await _context.Tenders.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
        if (tender == null) return false;

        // Domain method handles invariant state check (must be Published)
        tender.CloseTender();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> AwardTenderAsync(int id, int bidId, CancellationToken cancellationToken = default)
    {
        var tender = await _context.Tenders.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
        if (tender == null) return false;

        var bidExists = await _context.Bids.AnyAsync(b => b.Id == bidId && b.TenderId == id && !b.IsDeleted, cancellationToken);
        if (!bidExists)
        {
            throw new KeyNotFoundException($"Bid with ID {bidId} does not belong to Tender ID {id} or does not exist.");
        }

        // Domain method sets WinningBidId and switches status to Awarded
        tender.AwardToBid(bidId);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static TenderResponseDto MapToDto(Tender tender)
    {
        var documents = tender.Documents?
            .Select(MapToDocumentDto)
            .ToList() ?? new List<TenderDocumentDto>();

        return new TenderResponseDto(
            tender.Id,
            tender.ReferenceNumber,
            tender.Title,
            tender.Description,
            tender.EstimatedBudget,
            tender.PublicationDate,
            tender.SubmissionDeadline,
            tender.Status.ToString(),
            tender.CreatedByOfficerId,
            documents
        );
    }

    private static TenderDocumentDto MapToDocumentDto(TenderDocument doc)
    {
        return new TenderDocumentDto(
            doc.Id,
            doc.FileName,
            doc.FilePath,
            doc.UploadedAt
        );
    }
}