namespace TmsApi.Application.Auth.Commands.CreateSupplierProfile;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.Common.Interfaces;
using TmsApi.Application.Common.Models;
using TmsApi.Domain.Entities;

public record CreateSupplierProfileCommand(CreateSupplierProfileRequestDto ProfileDto) : IRequest<Result<int>>;

public class CreateSupplierProfileCommandHandler : IRequestHandler<CreateSupplierProfileCommand, Result<int>>
{
    private readonly ITmsDbContext _context;

    public CreateSupplierProfileCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<int>> Handle(CreateSupplierProfileCommand request, CancellationToken cancellationToken)
    {
        var dto = request.ProfileDto;

        var user = await _context.Users.FindAsync(new object[] { dto.UserId }, cancellationToken);
        if (user is null)
        {
            return Result<int>.Failure($"User with ID {dto.UserId} not found.");
        }

        var taxIdExists = await _context.SupplierProfiles.AnyAsync(sp => sp.TaxIdNumber == dto.TaxIdNumber, cancellationToken);
        if (taxIdExists)
        {
            return Result<int>.Failure($"Supplier profile with Tax ID '{dto.TaxIdNumber}' already exists.");
        }

        try
        {
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

            return Result<int>.Success(profile.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(ex.Message);
        }
    }
}