namespace ebr_powerbi.Application.Batches.Queries.GetBatchPdrsList;

public sealed class GetBatchPdrsListQueryValidator : AbstractValidator<GetBatchPdrsListQuery>
{
    private static readonly string[] Allowed = ["app", "wi", "app_master", "all"];

    public GetBatchPdrsListQueryValidator()
    {
        RuleFor(x => x.Type)
            .Must(t => string.IsNullOrWhiteSpace(t) || Allowed.Contains(t.Trim().ToLowerInvariant()))
            .WithMessage("Type must be app, wi, app_master, or all.");
    }
}
