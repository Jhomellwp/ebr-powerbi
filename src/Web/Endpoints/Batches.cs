using ebr_powerbi.Application.Batches.Queries.GetBatchPdrsList;
using ebr_powerbi.Application.Batches.Queries.GetBatchesList;
using ebr_powerbi.Application.Batches.Queries.GetBatchDetails;
using ebr_powerbi.Application.Batches.Queries.GetBatchAppWorksheet;
using ebr_powerbi.Application.Batches.Queries.GetBatchWiWorksheet;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ebr_powerbi.Web.Endpoints;

public sealed class Batches : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetBatches, string.Empty);
        groupBuilder.MapGet(GetPdrs, "pdrs");
        groupBuilder.MapGet(GetBatchDetails, "{id:int}");
        groupBuilder.MapGet(GetBatchAppWorksheet, "{id:int}/app-worksheet");
        groupBuilder.MapGet(GetBatchWiWorksheet, "{id:int}/wi-worksheet");
    }

    [EndpointSummary("List EBR batches")]
    [EndpointDescription("Like legacy EBR BatchList: optional `pdrs` narrows to that PDRS code. `type`: app | wi | app_master | omit=all.")]
    public static async Task<Ok<BatchesListVm>> GetBatches(ISender sender, string? type, string? pdrs)
    {
        var vm = await sender.Send(new GetBatchesListQuery(type, pdrs));
        return TypedResults.Ok(vm);
    }

    [EndpointSummary("PDRS filter values")]
    [EndpointDescription("Distinct PDRS codes for batches in scope (same `type` rules as list). For dropdown before calling GET /api/Batches with `pdrs`.")]
    public static async Task<Ok<PdrsListVm>> GetPdrs(ISender sender, string? type)
    {
        var vm = await sender.Send(new GetBatchPdrsListQuery(type));
        return TypedResults.Ok(vm);
    }

    [EndpointSummary("Batch details")]
    [EndpointDescription("Details payload for new in-app coversheet view.")]
    public static async Task<Results<Ok<BatchDetailsVm>, NotFound>> GetBatchDetails(ISender sender, int id)
    {
        var vm = await sender.Send(new GetBatchDetailsQuery(id));
        return vm is null ? TypedResults.NotFound() : TypedResults.Ok(vm);
    }

    [EndpointSummary("APP worksheet sections")]
    [EndpointDescription("PIF/WSM/PIW worksheet-like section data for in-app rendering.")]
    public static async Task<Ok<AppWorksheetVm>> GetBatchAppWorksheet(ISender sender, int id)
    {
        var vm = await sender.Send(new GetBatchAppWorksheetQuery(id));
        return TypedResults.Ok(vm);
    }

    [EndpointSummary("WI worksheet sections")]
    [EndpointDescription("WI section data for in-app rendering.")]
    public static async Task<Ok<WiWorksheetVm>> GetBatchWiWorksheet(ISender sender, int id)
    {
        var vm = await sender.Send(new GetBatchWiWorksheetQuery(id));
        return TypedResults.Ok(vm);
    }
}
