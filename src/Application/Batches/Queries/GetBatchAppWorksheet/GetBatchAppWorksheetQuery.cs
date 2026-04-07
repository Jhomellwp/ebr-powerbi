using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Application.Common.Security;

namespace ebr_powerbi.Application.Batches.Queries.GetBatchAppWorksheet;

[Authorize]
public sealed record GetBatchAppWorksheetQuery(int Id) : IRequest<AppWorksheetVm>;

public sealed record AppWorksheetVm(
    IReadOnlyList<AppWorksheetSectionVm> Sections);

public sealed record AppWorksheetSectionVm(
    string Name,
    string? Comments,
    IReadOnlyList<AppWorksheetSampleRowVm> Samples);

public sealed record AppWorksheetSampleRowVm(
    string? SampleNumber,
    string? Test,
    string? Specification,
    string? SpecificationForInformationOnly,
    string? Result);

public sealed class GetBatchAppWorksheetQueryHandler : IRequestHandler<GetBatchAppWorksheetQuery, AppWorksheetVm>
{
    private readonly IEbrBatchReadStore _batches;

    public GetBatchAppWorksheetQueryHandler(IEbrBatchReadStore batches)
    {
        _batches = batches;
    }

    public async Task<AppWorksheetVm> Handle(GetBatchAppWorksheetQuery request, CancellationToken cancellationToken)
    {
        var dto = await _batches.GetAppWorksheetAsync(request.Id, cancellationToken);
        return new AppWorksheetVm(dto.Sections.Select(Map).ToList());
    }

    private static AppWorksheetSectionVm Map(AppWorksheetSectionDto s)
    {
        return new AppWorksheetSectionVm(
            s.Name,
            s.Comments,
            s.Samples.Select(r => new AppWorksheetSampleRowVm(
                r.SampleNumber,
                r.Test,
                r.Specification,
                r.SpecificationForInformationOnly,
                r.Result)).ToList());
    }
}
