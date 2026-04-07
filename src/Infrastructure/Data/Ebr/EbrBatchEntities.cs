using System.ComponentModel.DataAnnotations.Schema;

namespace ebr_powerbi.Infrastructure.Data.Ebr;

/// <summary>Maps dbo.Batches (EBR legacy schema).</summary>
public sealed class EbrBatch
{
    public int Id { get; set; }
    public string? BatchNumber { get; set; }
    public int? PDRSID { get; set; }
    public int? FileTypeID { get; set; }
    public bool isActive { get; set; }
    public bool isArchive { get; set; }
    public string? HeaderTitle { get; set; }
    public string? Status { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? RetentionDueDate { get; set; }

    public EbrFileType? FileType { get; set; }
    public EbrPdrsNumber? PdrsNumber { get; set; }
}

/// <summary>Maps dbo.FileType.</summary>
public sealed class EbrFileType
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool isActive { get; set; }
}

/// <summary>Maps dbo.Coversheets — legacy EBR uses this for APP batch lists (not Coversheets_WI).</summary>
public sealed class EbrCoversheet
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public int? FileTypeID { get; set; }
    public bool isActive { get; set; }
    public string? ProductName { get; set; }

    public EbrBatch? Batch { get; set; }
    public EbrFileType? FileType { get; set; }
}

/// <summary>Maps dbo.Coversheets_WI — used for WI and APP Master (APP_Packing_Manual) in legacy EBR.</summary>
public sealed class EbrCoversheetsWi
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public int? FileTypeID { get; set; }
    public bool isActive { get; set; }
    public string? ProductName { get; set; }

    public EbrBatch? Batch { get; set; }
    public EbrFileType? FileType { get; set; }
}

/// <summary>Maps dbo.PDRSNumbers.</summary>
public sealed class EbrPdrsNumber
{
    public int Id { get; set; }

    [Column("PDRSNumber")]
    public string? PdrsNumberCode { get; set; }

    public int? LocationID { get; set; }
    public EbrLocation? Location { get; set; }
}

/// <summary>Maps dbo.Locations.</summary>
public sealed class EbrLocation
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public sealed class EbrPif
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrWsm
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrPiw
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrPifSample
{
    public int Id { get; set; }
    public int? PIFID { get; set; }
    public string? SampleNumber { get; set; }
    public string? Test { get; set; }
    public string? Specification { get; set; }
    public string? SpecificationForInformation { get; set; }
    public string? Result { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrWsmSample
{
    public int Id { get; set; }
    public int? WSMID { get; set; }
    public string? SampleNumber { get; set; }
    public string? Test { get; set; }
    public string? Specification { get; set; }
    public string? SpecificationForInformation { get; set; }
    public string? Result { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrPiwSample
{
    public int Id { get; set; }
    public int? PIWID { get; set; }
    public string? SampleNumber { get; set; }
    public string? Test { get; set; }
    public string? Specification { get; set; }
    public string? SpecificationForInformation { get; set; }
    public string? Result { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrBlend
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrWsvSln
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrFpt
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrUhtEvaporator
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrEvapParamater
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrEvapLogSheet
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrDryerParameter
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrDryingAndDryerLogSheet
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? Comments { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrPifWi
{
    public int Id { get; set; }
    public int? BatchID { get; set; }

    [Column("ObservationAndComments")]
    public string? ObservationAndComments { get; set; }
}

public sealed class EbrPiwWi
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
}

public sealed class EbrWsmWi
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
}

public sealed class EbrHydrationWi
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
}

public sealed class EbrFptWi
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
}

public sealed class EbrEvaporatorWi
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
}

public sealed class EbrDryerWi
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
}
