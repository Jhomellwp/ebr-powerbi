using ebr_powerbi.Application.Common.Interfaces;
using ebr_powerbi.Infrastructure.Data.Ebr;
using Microsoft.EntityFrameworkCore;

namespace ebr_powerbi.Infrastructure.Data;

/// <summary>
/// Loads batches the same way legacy EBR home does: APP via <c>dbo.Coversheets</c>,
/// WI and APP Master via <c>dbo.Coversheets_WI</c> with FileType names
/// <c>WI</c> and <c>APP_Packing_Manual</c> (see Abbott HomeController Index / BatchList).
/// </summary>
public sealed class EbrBatchReadStore : IEbrBatchReadStore
{
    /// <summary>Matches FileType.Name for the APP tab (<c>item.Coversheets</c> in EBR).</summary>
    private const string FileTypeNameApp = "APP";

    /// <summary>Matches FileType.Name for the WI tab (<c>item.Coversheets_WIs</c> on WI file type).</summary>
    private const string FileTypeNameWi = "WI";

    /// <summary>Matches EBR filter <c>a.FileType.Name == "APP_Packing_Manual"</c> for APP Master.</summary>
    private const string FileTypeNameAppPackingManual = "APP_Packing_Manual";

    private const int MaxPdrsOptions = 2000;

    private readonly ApplicationDbContext _db;

    public EbrBatchReadStore(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BatchListItemDto>> GetBatchesAsync(
        string? batchType,
        string? pdrsNumber,
        int take,
        CancellationToken cancellationToken)
    {
        var kind = NormalizeKind(batchType);
        var eligibleIds = EligibleBatchIds(kind);

        var query = _db.Set<EbrBatch>()
            .AsNoTracking()
            .Include(b => b.FileType)
            .Include(b => b.PdrsNumber)
            .ThenInclude(p => p!.Location)
            .Where(b => b.isActive && !b.isArchive && eligibleIds.Contains(b.Id));

        if (!string.IsNullOrWhiteSpace(pdrsNumber))
        {
            var code = pdrsNumber.Trim();
            query = query.Where(b => b.PdrsNumber != null && b.PdrsNumber.PdrsNumberCode == code);
        }

        var rows = await query
            .OrderByDescending(b => b.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        var batchIds = rows.ConvertAll(b => b.Id);
        var productByBatch = await LoadProductNamesForBatchesAsync(batchIds, kind, cancellationToken);

        return rows.ConvertAll(b => Map(b, kind, productByBatch.TryGetValue(b.Id, out var pn) ? pn : null));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetDistinctPdrsAsync(string? batchType, CancellationToken cancellationToken)
    {
        var kind = NormalizeKind(batchType);
        var eligibleIds = EligibleBatchIds(kind);

        return await _db.Set<EbrBatch>()
            .AsNoTracking()
            .Where(b => b.isActive && !b.isArchive && eligibleIds.Contains(b.Id) && b.PDRSID != null)
            .Join(_db.Set<EbrPdrsNumber>().AsNoTracking(),
                b => b.PDRSID,
                p => p.Id,
                (b, p) => p.PdrsNumberCode)
            .Where(c => c != null && c != "")
            .Select(c => c!)
            .Distinct()
            .OrderBy(c => c)
            .Take(MaxPdrsOptions)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BatchDetailsDto?> GetBatchDetailsAsync(int id, CancellationToken cancellationToken)
    {
        var batch = await _db.Set<EbrBatch>()
            .AsNoTracking()
            .Include(b => b.FileType)
            .Include(b => b.PdrsNumber)
            .ThenInclude(p => p!.Location)
            .FirstOrDefaultAsync(b => b.Id == id && b.isActive, cancellationToken);

        if (batch is null)
        {
            return null;
        }

        var product = await ResolveBatchProductNameAsync(batch.Id, cancellationToken);

        return new BatchDetailsDto(
            batch.Id,
            batch.BatchNumber ?? string.Empty,
            batch.PdrsNumber?.PdrsNumberCode ?? string.Empty,
            product,
            batch.HeaderTitle,
            batch.Status,
            batch.ReleaseDate,
            batch.RetentionDueDate,
            batch.FileType?.Name,
            batch.PdrsNumber?.Location?.Name,
            batch.isArchive);
    }

    /// <inheritdoc />
    public async Task<AppWorksheetDto> GetAppWorksheetAsync(int batchId, CancellationToken cancellationToken)
    {
        var pif = await _db.Set<EbrPif>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var wsm = await _db.Set<EbrWsm>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var piw = await _db.Set<EbrPiw>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);

        var pifSamples = pif is null
            ? new List<AppWorksheetSampleRowDto>()
            : await _db.Set<EbrPifSample>().AsNoTracking()
                .Where(x => x.PIFID == pif.Id)
                .OrderBy(x => x.Id)
                .Select(x => new AppWorksheetSampleRowDto(
                    x.SampleNumber,
                    x.Test,
                    x.Specification,
                    x.SpecificationForInformation,
                    x.Result))
                .ToListAsync(cancellationToken);

        var wsmSamples = wsm is null
            ? new List<AppWorksheetSampleRowDto>()
            : await _db.Set<EbrWsmSample>().AsNoTracking()
                .Where(x => x.WSMID == wsm.Id)
                .OrderBy(x => x.Id)
                .Select(x => new AppWorksheetSampleRowDto(
                    x.SampleNumber,
                    x.Test,
                    x.Specification,
                    x.SpecificationForInformation,
                    x.Result))
                .ToListAsync(cancellationToken);

        var piwSamples = piw is null
            ? new List<AppWorksheetSampleRowDto>()
            : await _db.Set<EbrPiwSample>().AsNoTracking()
                .Where(x => x.PIWID == piw.Id)
                .OrderBy(x => x.Id)
                .Select(x => new AppWorksheetSampleRowDto(
                    x.SampleNumber,
                    x.Test,
                    x.Specification,
                    x.SpecificationForInformation,
                    x.Result))
                .ToListAsync(cancellationToken);

        var blend = await _db.Set<EbrBlend>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var wsv = await _db.Set<EbrWsvSln>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var fpt = await _db.Set<EbrFpt>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var uht = await _db.Set<EbrUhtEvaporator>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var dryer = await _db.Set<EbrDryerParameter>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var drying = await _db.Set<EbrDryingAndDryerLogSheet>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);

        var blendSamples = blend is null
            ? new List<EbrBlendSampleTable>()
            : await _db.Set<EbrBlendSampleTable>().AsNoTracking()
                .Where(x => x.BlendID == blend.Id)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);

        var wsvSamples = wsv is null
            ? new List<AppWorksheetSampleRowDto>()
            : await _db.Set<EbrWsvSlnSampleTable>().AsNoTracking()
                .Where(x => x.WSVSlnID == wsv.Id)
                .OrderBy(x => x.Id)
                .Select(x => new AppWorksheetSampleRowDto(
                    x.SampleNumber,
                    x.Test,
                    x.Specification,
                    x.SpecificationForInformation,
                    x.Result))
                .ToListAsync(cancellationToken);

        var fptSamples = fpt is null
            ? new List<EbrFptSampleTable>()
            : await _db.Set<EbrFptSampleTable>().AsNoTracking()
                .Where(x => x.FPTID == fpt.Id)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);

        var uhtSamples = uht is null
            ? new List<AppWorksheetSampleRowDto>()
            : await _db.Set<EbrUhtSampleTable>().AsNoTracking()
                .Where(x => x.UHTEvaporatorID == uht.Id)
                .OrderBy(x => x.Id)
                .Select(x => new AppWorksheetSampleRowDto(
                    x.Time,
                    x.HMIDensity,
                    x.TotalSolids,
                    x.Viscosity,
                    JoinParts("Temp:", x.Viscosity2, "Torque:", x.Viscosity4, "Speed:", x.Viscosity5)))
                .ToListAsync(cancellationToken);

        var dryerRows = dryer is null
            ? new List<AppWorksheetSampleRowDto>()
            : await (
                    from row in _db.Set<EbrDryerParameterTable>().AsNoTracking()
                    join param in _db.Set<EbrDryerParameterParameter>().AsNoTracking()
                        on row.ParameterID equals param.Id into pset
                    from param in pset.DefaultIfEmpty()
                    where row.DryerParameterID == dryer.Id
                    orderby row.Id
                    select new AppWorksheetSampleRowDto(
                        param != null ? param.Parameter : null,
                        row.UOM,
                        row.PresetValue,
                        row.PreTrialValue,
                        row.PostTrialValue))
                .ToListAsync(cancellationToken);

        var dryingPerformRows = drying is null
            ? new List<AppWorksheetSampleRowDto>()
            : await _db.Set<EbrDryingAndDryerPerformTable>().AsNoTracking()
                .Where(x => x.DryingAndDryerLogSheetID == drying.Id)
                .OrderBy(x => x.Id)
                .Select(x => new AppWorksheetSampleRowDto(
                    x.SampleNo,
                    x.Test,
                    x.Specification,
                    x.BOB,
                    JoinParts("MOB:", x.MOB, "EOB:", x.EOB)))
                .ToListAsync(cancellationToken);

        var dryingLogRows = drying is null
            ? new List<AppWorksheetSampleRowDto>()
            : await _db.Set<EbrDryingAndDryerLogSheetTable>().AsNoTracking()
                .Where(x => x.DryingAndDryerLogSheetID == drying.Id)
                .OrderBy(x => x.Id)
                .Select(x => new AppWorksheetSampleRowDto(
                    x.Description,
                    x.Unit,
                    x.Freq,
                    x.Product1,
                    JoinParts(x.Product2, x.Product3, x.Product4, x.Product5, x.Product6)))
                .ToListAsync(cancellationToken);

        var dryingCollectRows = drying is null
            ? new List<AppWorksheetSampleRowDto>()
            : await _db.Set<EbrDryingAndDryerCollectSampleTable>().AsNoTracking()
                .Where(x => x.DryingAndDryerLogSheetID == drying.Id)
                .OrderBy(x => x.Id)
                .Select(x => new AppWorksheetSampleRowDto(
                    x.SampleNo,
                    "Bulk Density",
                    null,
                    null,
                    x.BulkDensity))
                .ToListAsync(cancellationToken);

        var sections = new List<AppWorksheetSectionDto>
        {
            new("PIF", pif?.Comments, pifSamples),
            new("WSM", wsm?.Comments, wsmSamples),
            new("PIW", piw?.Comments, piwSamples),
            new("Blending", blend?.Comments, BuildBlendRows(blend, blendSamples)),
            new("WSV Solution", wsv?.Comments, wsvSamples),
            new("FPT", fpt?.Comments, BuildFptRows(fpt, fptSamples)),
            new("UHT Evaporator", uht?.Comments, uhtSamples),
            new("Dryer Parameters", null, dryerRows),
            new("Drying and Dryer Log", null, BuildDryingRows(drying, dryingPerformRows, dryingLogRows, dryingCollectRows))
        };

        return new AppWorksheetDto(sections);
    }

    private static IReadOnlyList<AppWorksheetSampleRowDto> BuildBlendRows(EbrBlend? blend, IReadOnlyList<EbrBlendSampleTable> sampleRows)
    {
        var rows = new List<AppWorksheetSampleRowDto>
        {
            new("pH specification (at 25 degC)", blend?._4PhSpecification, null, null, null),
            new("Target pH (at 25 degC)", blend?._4TargetPh, null, null, null),
            new("pH before correction (at 25 degC)", blend?._4PhBeforeCorrection, null, null, null),
            new("pH correction needed?", blend?._4PhCorrectionNeeded, null, null, null),
            new("Final pH after correction", blend?._4FinalPHAfterCorrection, null, null, null),
            new("Weight of Sample Drawn (g)", blend?.WeightOfSampleDrawnA, blend?.WeightOfSampleDrawnB, blend?.WeightOfSampleDrawnC, null),
            new("Weight of 5% KOH used (g)", blend?.WeightOfKOGUsedA, blend?.WeightOfKOGUsedB, blend?.WeightOfKOGUsedC, null),
            new("Weight of Hydration Tank (kg)", blend?.WeightOfFPTankA, blend?.WeightOfFPTankB, blend?.WeightOfFPTankC, null),
            new("Weight of 5% KOH needed for tank (kg)", blend?.WeightOfKONeededA, blend?.WeightOfKONeededB, blend?.WeightOfKONeededC, null),
            new("pH after correction", blend?.PhAfterCorrectionA, blend?.PhAfterCorrectionB, blend?.PhAfterCorrectionC, null),
            new("Actual Quantity weighed (kg)", blend?.PhRecordedByIdA?.ToString(), blend?.PhRecordedByIdB?.ToString(), blend?.PhRecordedByIdC?.ToString(), null)
        };

        for (var i = 0; i < sampleRows.Count; i++)
        {
            var s = sampleRows[i];
            rows.Add(new(
                string.IsNullOrWhiteSpace(s.SampleNumber) ? $"Blend/Hydration Sample Row {i + 1}" : s.SampleNumber,
                s.Test,
                s.Specification,
                s.SpecificationForInformation,
                JoinParts("HYD-1:", s.ResultHYD1, "HYD-2:", s.ResultHYD2, "Result:", s.Result)));
        }

        return rows;
    }

    private static IReadOnlyList<AppWorksheetSampleRowDto> BuildFptRows(EbrFpt? fpt, IReadOnlyList<EbrFptSampleTable> sampleRows)
    {
        var rows = new List<AppWorksheetSampleRowDto>
        {
            new("pH specification (at 25 degC)", fpt?._2PhSpecification, null, null, null),
            new("Target pH (at 25 degC)", fpt?._2TargetPh, null, null, null),
            new("pH before correction (at 25 degC)", fpt?._2PhBeforeCorrection, null, null, null),
            new("pH correction needed?", fpt?._2PhCorrectionNeeded, null, null, null),
            new("Final pH after correction", fpt?._2PhAfterCorrection, null, null, null),
            new("Weight of Sample Drawn (g)", fpt?.WeightOfSampleDrawnA, fpt?.WeightOfSampleDrawnB, fpt?.WeightOfSampleDrawnC, null),
            new("Weight of 5% KOH used (g)", fpt?.WeightOfKOGUsedA, fpt?.WeightOfKOGUsedB, fpt?.WeightOfKOGUsedC, null),
            new("Weight of Hydration Tank (kg)", fpt?.WeightOfFPTankA, fpt?.WeightOfFPTankB, fpt?.WeightOfFPTankC, null),
            new("Weight of 5% KOH needed for tank (kg)", fpt?.WeightOfKONeededA, fpt?.WeightOfKONeededB, fpt?.WeightOfKONeededC, null),
            new("pH after correction", fpt?.PhAfterCorrectionA, fpt?.PhAfterCorrectionB, fpt?.PhAfterCorrectionC, null),
            new("Actual Quantity weighed (kg)", fpt?.PhRecordedByIdA?.ToString(), fpt?.PhRecordedByIdB?.ToString(), fpt?.PhRecordedByIdC?.ToString(), null)
        };

        rows.AddRange(sampleRows.Select(s => new AppWorksheetSampleRowDto(
            s.SampleNumber,
            s.Test,
            s.Specification,
            s.SpecificationForInformation,
            JoinParts("Result:", s.Result, "Result2:", s.Result2))));

        return rows;
    }

    private static IReadOnlyList<AppWorksheetSampleRowDto> BuildDryingRows(
        EbrDryingAndDryerLogSheet? drying,
        IReadOnlyList<AppWorksheetSampleRowDto> performRows,
        IReadOnlyList<AppWorksheetSampleRowDto> logRows,
        IReadOnlyList<AppWorksheetSampleRowDto> collectRows)
    {
        var rows = new List<AppWorksheetSampleRowDto>
        {
            new("Orifice Size", drying?.OrificeSize, null, null, null),
            new("Core", drying?.Core, null, null, null),
            new("Lance Position", drying?.LancePosition, null, null, null),
            new("Hourly Parameter Water/Product", drying?.Water, drying?.Product1, drying?.Product2, JoinParts(drying?.Product3, drying?.Product4, drying?.Product5, drying?.Product6)),
            new("End Time", drying?.EndTime, null, null, null)
        };

        rows.AddRange(performRows);
        rows.AddRange(logRows);
        rows.AddRange(collectRows);
        return rows;
    }

    private static string? JoinParts(params string?[] values)
    {
        var parts = values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!.Trim())
            .ToList();

        return parts.Count == 0 ? null : string.Join(" | ", parts);
    }

    /// <inheritdoc />
    public async Task<AppWorksheetDto> GetWiWorksheetAsync(int batchId, CancellationToken cancellationToken)
    {
        var pif = await _db.Set<EbrPifWi>().AsNoTracking().FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var piw = await _db.Set<EbrPiwWi>().AsNoTracking().FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var wsm = await _db.Set<EbrWsmWi>().AsNoTracking().FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var hydration = await _db.Set<EbrHydrationWi>().AsNoTracking().FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var fpt = await _db.Set<EbrFptWi>().AsNoTracking().FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var evap = await _db.Set<EbrEvaporatorWi>().AsNoTracking().FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);
        var dryer = await _db.Set<EbrDryerWi>().AsNoTracking().FirstOrDefaultAsync(x => x.BatchID == batchId, cancellationToken);

        var sections = new List<AppWorksheetSectionDto>
        {
            new("PIF (WI)", pif?.ObservationAndComments, []),
            new("PIW (WI)", null, []),
            new("WSM (WI)", null, []),
            new("Hydration (WI)", null, []),
            new("FPT (WI)", null, []),
            new("Evaporator (WI)", null, []),
            new("Dryer (WI)", null, [])
        };

        return new AppWorksheetDto(sections);
    }

    private static string? NormalizeKind(string? batchType)
    {
        if (string.IsNullOrWhiteSpace(batchType) || batchType.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return batchType.Trim().ToLowerInvariant();
    }

    private IQueryable<int> EligibleBatchIds(string? normalizedKind)
    {
        return normalizedKind switch
        {
            "app" => BatchIdsWithActiveAppCoversheet(),
            "wi" => BatchIdsWithActiveWiCoversheet(FileTypeNameWi),
            "app_master" => BatchIdsWithActiveWiCoversheet(FileTypeNameAppPackingManual),
            _ => BatchIdsWithActiveAppCoversheet()
                .Union(BatchIdsWithActiveWiCoversheet(FileTypeNameWi))
                .Union(BatchIdsWithActiveWiCoversheet(FileTypeNameAppPackingManual))
        };
    }

    /// <summary>EBR: <c>item.Coversheets.Where(a =&gt; a.isActive &amp;&amp; !a.Batch.isArchive)</c> for APP FileType.</summary>
    private IQueryable<int> BatchIdsWithActiveAppCoversheet()
    {
        return _db.Set<EbrCoversheet>()
            .AsNoTracking()
            .Where(cs => cs.isActive && cs.BatchID != null && cs.FileTypeID != null)
            .Join(_db.Set<EbrBatch>().AsNoTracking(),
                cs => cs.BatchID,
                b => b.Id,
                (cs, b) => new { cs, b })
            .Where(x => x.b.isActive && !x.b.isArchive)
            .Join(_db.Set<EbrFileType>().AsNoTracking(),
                x => x.cs.FileTypeID,
                ft => ft.Id,
                (x, ft) => new { x.b, ft })
            .Where(x => x.ft.isActive && x.ft.Name == FileTypeNameApp)
            .Select(x => x.b.Id);
    }

    /// <summary>EBR: <c>Coversheets_WI</c> with matching FileType name (WI or APP_Packing_Manual).</summary>
    private IQueryable<int> BatchIdsWithActiveWiCoversheet(string fileTypeName)
    {
        return _db.Set<EbrCoversheetsWi>()
            .AsNoTracking()
            .Where(wi => wi.isActive && wi.BatchID != null && wi.FileTypeID != null)
            .Join(_db.Set<EbrBatch>().AsNoTracking(),
                wi => wi.BatchID,
                b => b.Id,
                (wi, b) => new { wi, b })
            .Where(x => x.b.isActive && !x.b.isArchive)
            .Join(_db.Set<EbrFileType>().AsNoTracking(),
                x => x.wi.FileTypeID,
                ft => ft.Id,
                (x, ft) => new { x.b, ft })
            .Where(x => x.ft.isActive && x.ft.Name == fileTypeName)
            .Select(x => x.b.Id);
    }

    private async Task<Dictionary<int, string?>> LoadProductNamesForBatchesAsync(
        List<int> batchIds,
        string? normalizedKind,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<int, string?>();
        if (batchIds.Count == 0)
        {
            return result;
        }

        async Task AddFromAppCoversheetsAsync()
        {
            var rows = await (
                    from cs in _db.Set<EbrCoversheet>().AsNoTracking()
                    join ft in _db.Set<EbrFileType>().AsNoTracking() on cs.FileTypeID equals ft.Id
                    where cs.isActive
                          && cs.BatchID != null
                          && batchIds.Contains(cs.BatchID.Value)
                          && ft.isActive
                          && ft.Name == FileTypeNameApp
                    select new { BatchId = cs.BatchID!.Value, cs.ProductName })
                .ToListAsync(cancellationToken);

            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.ProductName))
                {
                    continue;
                }

                result[r.BatchId] = r.ProductName;
            }
        }

        async Task AddFromWiCoversheetsAsync(string fileTypeName)
        {
            var rows = await (
                    from wi in _db.Set<EbrCoversheetsWi>().AsNoTracking()
                    join ft in _db.Set<EbrFileType>().AsNoTracking() on wi.FileTypeID equals ft.Id
                    where wi.isActive
                          && wi.BatchID != null
                          && batchIds.Contains(wi.BatchID.Value)
                          && ft.isActive
                          && ft.Name == fileTypeName
                    select new { BatchId = wi.BatchID!.Value, wi.ProductName })
                .ToListAsync(cancellationToken);

            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.ProductName) || result.ContainsKey(r.BatchId))
                {
                    continue;
                }

                result[r.BatchId] = r.ProductName;
            }
        }

        switch (normalizedKind)
        {
            case "app":
                await AddFromAppCoversheetsAsync();
                break;
            case "wi":
                await AddFromWiCoversheetsAsync(FileTypeNameWi);
                break;
            case "app_master":
                await AddFromWiCoversheetsAsync(FileTypeNameAppPackingManual);
                break;
            default:
                await AddFromAppCoversheetsAsync();
                await AddFromWiCoversheetsAsync(FileTypeNameWi);
                await AddFromWiCoversheetsAsync(FileTypeNameAppPackingManual);
                break;
        }

        return result;
    }

    private async Task<string?> ResolveBatchProductNameAsync(int batchId, CancellationToken cancellationToken)
    {
        var appProduct = await (
                from cs in _db.Set<EbrCoversheet>().AsNoTracking()
                join ft in _db.Set<EbrFileType>().AsNoTracking() on cs.FileTypeID equals ft.Id
                where cs.isActive
                      && cs.BatchID == batchId
                      && ft.isActive
                      && ft.Name == FileTypeNameApp
                select cs.ProductName)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(appProduct))
        {
            return appProduct.Trim();
        }

        var wiProduct = await (
                from wi in _db.Set<EbrCoversheetsWi>().AsNoTracking()
                join ft in _db.Set<EbrFileType>().AsNoTracking() on wi.FileTypeID equals ft.Id
                where wi.isActive
                      && wi.BatchID == batchId
                      && ft.isActive
                      && (ft.Name == FileTypeNameWi || ft.Name == FileTypeNameAppPackingManual)
                select wi.ProductName)
            .FirstOrDefaultAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(wiProduct))
        {
            return wiProduct.Trim();
        }

        return null;
    }

    private static BatchListItemDto Map(EbrBatch b, string? filterKind, string? productName)
    {
        var pdrs = b.PdrsNumber?.PdrsNumberCode ?? "";
        var batchNo = b.BatchNumber ?? "";

        var displayProduct = !string.IsNullOrWhiteSpace(productName)
            ? productName.Trim()
            : !string.IsNullOrWhiteSpace(b.HeaderTitle)
                ? b.HeaderTitle.Trim()
                : null;

        var fileTypeLabel = filterKind switch
        {
            "app" => FileTypeNameApp,
            "wi" => FileTypeNameWi,
            "app_master" => FileTypeNameAppPackingManual,
            _ => b.FileType?.Name
        };

        return new BatchListItemDto(
            b.Id,
            batchNo,
            pdrs,
            displayProduct,
            b.Status,
            b.ReleaseDate,
            b.RetentionDueDate,
            fileTypeLabel);
    }

}
