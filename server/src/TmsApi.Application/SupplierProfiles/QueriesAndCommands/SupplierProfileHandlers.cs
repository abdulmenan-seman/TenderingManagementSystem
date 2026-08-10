namespace TmsApi.Application.SupplierProfiles.QueriesAndCommands;

using MediatR;
using TmsApi.Application.Auth.DTOs;
using TmsApi.Application.Common.Interfaces;

public record CreateSupplierProfileCommand(CreateSupplierProfileRequestDto Dto) : IRequest<SupplierProfileResponseDto>;
public record GetSupplierProfileByIdQuery(int Id) : IRequest<SupplierProfileResponseDto?>;
public record GetSupplierProfileByUserIdQuery(int UserId) : IRequest<SupplierProfileResponseDto?>;
public record GetAllSupplierProfilesQuery() : IRequest<IEnumerable<SupplierProfileResponseDto>>;

public class SupplierProfileHandler :
    IRequestHandler<CreateSupplierProfileCommand, SupplierProfileResponseDto>,
    IRequestHandler<GetSupplierProfileByIdQuery, SupplierProfileResponseDto?>,
    IRequestHandler<GetSupplierProfileByUserIdQuery, SupplierProfileResponseDto?>,
    IRequestHandler<GetAllSupplierProfilesQuery, IEnumerable<SupplierProfileResponseDto>>
{
    private readonly ISupplierProfileService _supplierProfileService;

    public SupplierProfileHandler(ISupplierProfileService supplierProfileService) 
        => _supplierProfileService = supplierProfileService;

    public Task<SupplierProfileResponseDto> Handle(CreateSupplierProfileCommand request, CancellationToken cancellationToken)
        => _supplierProfileService.CreateProfileAsync(request.Dto, cancellationToken);

    public Task<SupplierProfileResponseDto?> Handle(GetSupplierProfileByIdQuery request, CancellationToken cancellationToken)
        => _supplierProfileService.GetByIdAsync(request.Id, cancellationToken);

    public Task<SupplierProfileResponseDto?> Handle(GetSupplierProfileByUserIdQuery request, CancellationToken cancellationToken)
        => _supplierProfileService.GetByUserIdAsync(request.UserId, cancellationToken);

    public Task<IEnumerable<SupplierProfileResponseDto>> Handle(GetAllSupplierProfilesQuery request, CancellationToken cancellationToken)
        => _supplierProfileService.GetAllAsync(cancellationToken);
}