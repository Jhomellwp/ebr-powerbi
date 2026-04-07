using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Application.Common.Security;

namespace ebr_powerbi.Application.Batches.Queries.GetBatchesList;

[Authorize]
public sealed record GetBatchesListQuery(string? Type, string? Pdrs) : IRequest<BatchesListVm>;

public sealed record BatchesListVm(IReadOnlyList<BatchListItemDto> Items);

public sealed class GetBatchesListQueryHandler : IRequestHandler<GetBatchesListQuery, BatchesListVm>
{
    private const int DefaultTake = 750;
    private readonly IEbrBatchReadStore _batches;

    public GetBatchesListQueryHandler(IEbrBatchReadStore batches)
    {
        _batches = batches;
    }

    public async Task<BatchesListVm> Handle(GetBatchesListQuery request, CancellationToken cancellationToken)
    {
        var t = string.IsNullOrWhiteSpace(request.Type)
            ? null
            : request.Type.Trim().ToLowerInvariant();
        var normalized = t is null or "all" ? null : t;
        var pdrs = string.IsNullOrWhiteSpace(request.Pdrs) ? null : request.Pdrs.Trim();
        var items = await _batches.GetBatchesAsync(normalized, pdrs, DefaultTake, cancellationToken);
        return new BatchesListVm(items);
    }
}
