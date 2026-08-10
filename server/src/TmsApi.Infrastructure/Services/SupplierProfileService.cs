namespace TmsApi.Infrastructure.Services;

using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence;

public class SupplierProfileService : ISupplierProfileService
{
    private readonly TmsDbContext _context;

    public SupplierProfileService(TmsDbContext context)
    {
        _context = context;
    }

    public async Task<SupplierProfileResponseDto> CreateProfileAsync(CreateSupplierProfileRequestDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Verify User exists
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId && !u.IsDeleted, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {dto.UserId} does not exist.");
        }

        // 2. Check if profile already exists for this user
        var existingProfile = await _context.SupplierProfiles.AnyAsync(sp => sp.UserId == dto.UserId, cancellationToken);
        if (existingProfile)
        {
            throw new InvalidOperationException($"Supplier profile already exists for User ID {dto.UserId}.");
        }

        // 3. Create using Domain Constructor
        var profile = new SupplierProfile(
            dto.UserId,
            dto.CompanyName,
            dto.TaxIdNumber,
            dto.BusinessLicenseNumber,
            dto.Address,
            dto.PhoneNumber
        );

        _context.SupplierProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(profile);
    }

    public async Task<SupplierProfileResponseDto?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        return profile == null ? null : MapToDto(profile);
    }

    public async Task<SupplierProfileResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);

        return profile == null ? null : MapToDto(profile);
    }

    public async Task<IEnumerable<SupplierProfileResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var profiles = await _context.SupplierProfiles.ToListAsync(cancellationToken);
        return profiles.Select(MapToDto);
    }

    private static SupplierProfileResponseDto MapToDto(SupplierProfile profile)
    {
        return new SupplierProfileResponseDto(
            profile.Id,
            profile.UserId,
            profile.CompanyName,
            profile.TaxIdNumber,
            profile.BusinessLicenseNumber,
            profile.Address,
            profile.PhoneNumber
        );
    }
}