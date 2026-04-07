using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Application.Common.Security;

namespace ebr_powerbi.Application.Batches.Queries.GetBatchWiWorksheet;

[Authorize]
public sealed record GetBatchWiWorksheetQuery(int Id) : IRequest<WiWorksheetVm>;

public sealed record WiWorksheetVm(
    IReadOnlyList<WiWorksheetSectionVm> Sections);

public sealed record WiWorksheetSectionVm(
    string Name,
    string? Comments,
    IReadOnlyList<WiWorksheetSampleRowVm> Samples);

public sealed record WiWorksheetSampleRowVm(
    string? SampleNumber,
    string? Test,
    string? Specification,
    string? SpecificationForInformationOnly,
    string? Result);

public sealed class GetBatchWiWorksheetQueryHandler : IRequestHandler<GetBatchWiWorksheetQuery, WiWorksheetVm>
{
    private readonly IEbrBatchReadStore _batches;

    public GetBatchWiWorksheetQueryHandler(IEbrBatchReadStore batches)
    {
        _batches = batches;
    }

    public async Task<WiWorksheetVm> Handle(GetBatchWiWorksheetQuery request, CancellationToken cancellationToken)
    {
        var dto = await _batches.GetWiWorksheetAsync(request.Id, cancellationToken);
        return new WiWorksheetVm(dto.Sections.Select(s =>
            new WiWorksheetSectionVm(
                s.Name,
                s.Comments,
                s.Samples.Select(r => new WiWorksheetSampleRowVm(
                    r.SampleNumber,
                    r.Test,
                    r.Specification,
                    r.SpecificationForInformationOnly,
                    r.Result)).ToList())).ToList());
    }
}
