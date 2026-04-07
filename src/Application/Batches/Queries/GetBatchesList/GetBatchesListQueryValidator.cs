namespace ebr_powerbi.Application.Batches.Queries.GetBatchesList;

public sealed class GetBatchesListQueryValidator : AbstractValidator<GetBatchesListQuery>
{
    private static readonly string[] Allowed = ["app", "wi", "app_master", "all"];

    public GetBatchesListQueryValidator()
    {
        RuleFor(x => x.Type)
            .Must(t => string.IsNullOrWhiteSpace(t) || Allowed.Contains(t.Trim().ToLowerInvariant()))
            .WithMessage("Type must be app, wi, app_master, or all.");
    }
}
