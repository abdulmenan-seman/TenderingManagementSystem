namespace TmsApi.Application.Tenders.QueriesAndCommands;

using MediatR;
using TmsApi.Application.Common.Models;
using TmsApi.Application.Tenders.DTOs;

// Command to create a tender (OfficerId resolved server-side from JWT)
public record CreateTenderCommand(CreateTenderRequestDto Dto, int OfficerId) : IRequest<Result<TenderResponseDto>>;

// Query to get a tender by ID
public record GetTenderByIdQuery(int Id) : IRequest<Result<TenderResponseDto>>;

// Command to publish a draft tender
public record PublishTenderCommand(int Id) : IRequest<Result<bool>>;

// Command to get paginated list of tenders
public record GetTendersQuery(int PageNumber, int PageSize, string? Status) : IRequest<Result<PaginatedTendersDto>>;

// Command to update a draft tender
public record UpdateTenderCommand(int Id, CreateTenderRequestDto Dto) : IRequest<Result<TenderResponseDto>>;

// Command to delete a draft tender
public record DeleteTenderCommand(int Id) : IRequest<Result<bool>>;