using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Application.Common.Security;

namespace ebr_powerbi.Application.Batches.Queries.GetBatchDetails;

[Authorize]
public sealed record GetBatchDetailsQuery(int Id) : IRequest<BatchDetailsVm?>;

public sealed record BatchDetailsVm(
    int Id,
    string BatchNumber,
    string PdrsNumber,
    string? ProductName,
    string? HeaderTitle,
    string? Status,
    DateTime? ReleaseDate,
    DateTime? RetentionDueDate,
    string? FileTypeName,
    string? LocationName,
    bool IsArchived);

public sealed class GetBatchDetailsQueryHandler : IRequestHandler<GetBatchDetailsQuery, BatchDetailsVm?>
{
    private readonly IEbrBatchReadStore _batches;

    public GetBatchDetailsQueryHandler(IEbrBatchReadStore batches)
    {
        _batches = batches;
    }

    public async Task<BatchDetailsVm?> Handle(GetBatchDetailsQuery request, CancellationToken cancellationToken)
    {
        var dto = await _batches.GetBatchDetailsAsync(request.Id, cancellationToken);
        if (dto is null)
        {
            return null;
        }

        return new BatchDetailsVm(
            dto.Id,
            dto.BatchNumber,
            dto.PdrsNumber,
            dto.ProductName,
            dto.HeaderTitle,
            dto.Status,
            dto.ReleaseDate,
            dto.RetentionDueDate,
            dto.FileTypeName,
            dto.LocationName,
            dto.IsArchived);
    }
}
