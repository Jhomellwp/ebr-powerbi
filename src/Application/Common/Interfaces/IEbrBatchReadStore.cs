namespace ebr_powerbi.Application.Common.Interfaces;

/// <summary>
/// Read-only access to EBR-style batch rows from the shared SQL database (Batches, FileType, PDRS, Location).
/// </summary>
public interface IEbrBatchReadStore
{
    /// <summary>
    /// Optional filter: <c>app</c>, <c>wi</c>, <c>app_master</c> (case-insensitive). Null = union of all three workflows (like EBR tabs combined).
    /// </summary>
    /// <param name="pdrsNumber">When set, only batches whose PDRS code matches (legacy EBR BatchList filter).</param>
    Task<IReadOnlyList<BatchListItemDto>> GetBatchesAsync(string? batchType, string? pdrsNumber, int take, CancellationToken cancellationToken);

    /// <summary>Distinct PDRS codes for batches eligible under <paramref name="batchType"/>, ordered ascending.</summary>
    Task<IReadOnlyList<string>> GetDistinctPdrsAsync(string? batchType, CancellationToken cancellationToken);

    /// <summary>Batch details used by the new in-app coversheet-style details page.</summary>
    Task<BatchDetailsDto?> GetBatchDetailsAsync(int id, CancellationToken cancellationToken);

    /// <summary>APP worksheet-style sections (parity subset with legacy ExportAPP workbook).</summary>
    Task<AppWorksheetDto> GetAppWorksheetAsync(int batchId, CancellationToken cancellationToken);

    /// <summary>WI worksheet-style sections (parity subset with legacy ExportWI workbook).</summary>
    Task<AppWorksheetDto> GetWiWorksheetAsync(int batchId, CancellationToken cancellationToken);
}

/// <summary>Batch list row: aligns with legacy EBR <c>_BatchList</c> (batch #, product name, PDRS, status, dates).</summary>
public sealed record BatchListItemDto(
    int Id,
    string BatchNumber,
    string PdrsNumber,
    string? ProductName,
    string? Status,
    DateTime? ReleaseDate,
    DateTime? RetentionDueDate,
    string? FileTypeName);

public sealed record BatchDetailsDto(
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

public sealed record AppWorksheetDto(
    IReadOnlyList<AppWorksheetSectionDto> Sections);

public sealed record AppWorksheetSectionDto(
    string Name,
    string? Comments,
    IReadOnlyList<AppWorksheetSampleRowDto> Samples);

public sealed record AppWorksheetSampleRowDto(
    string? SampleNumber,
    string? Test,
    string? Specification,
    string? SpecificationForInformationOnly,
    string? Result);
