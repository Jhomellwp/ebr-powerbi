using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Application.Common.Security;

namespace ebr_powerbi.Application.Batches.Queries.GetBatchPdrsList;

[Authorize]
public sealed record GetBatchPdrsListQuery(string? Type) : IRequest<PdrsListVm>;

public sealed record PdrsListVm(IReadOnlyList<string> Items);

public sealed class GetBatchPdrsListQueryHandler : IRequestHandler<GetBatchPdrsListQuery, PdrsListVm>
{
    private readonly IEbrBatchReadStore _batches;

    public GetBatchPdrsListQueryHandler(IEbrBatchReadStore batches)
    {
        _batches = batches;
    }

    public async Task<PdrsListVm> Handle(GetBatchPdrsListQuery request, CancellationToken cancellationToken)
    {
        var t = string.IsNullOrWhiteSpace(request.Type)
            ? null
            : request.Type.Trim().ToLowerInvariant();
        var normalized = t is null or "all" ? null : t;
        var items = await _batches.GetDistinctPdrsAsync(normalized, cancellationToken);
        return new PdrsListVm(items);
    }
}
