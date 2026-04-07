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
    [Column("4PhSpecification")]
    public string? _4PhSpecification { get; set; }
    [Column("4TargetpH")]
    public string? _4TargetPh { get; set; }
    [Column("4PhBeforeCorrection")]
    public string? _4PhBeforeCorrection { get; set; }
    [Column("4PhCorrectionNeeded")]
    public string? _4PhCorrectionNeeded { get; set; }
    [Column("4FinalPHAfterCorrection")]
    public string? _4FinalPHAfterCorrection { get; set; }
    public string? WeightOfSampleDrawnA { get; set; }
    public string? WeightOfSampleDrawnB { get; set; }
    public string? WeightOfSampleDrawnC { get; set; }
    public string? WeightOfKOGUsedA { get; set; }
    public string? WeightOfKOGUsedB { get; set; }
    public string? WeightOfKOGUsedC { get; set; }
    public string? WeightOfFPTankA { get; set; }
    public string? WeightOfFPTankB { get; set; }
    public string? WeightOfFPTankC { get; set; }
    public string? WeightOfKONeededA { get; set; }
    public string? WeightOfKONeededB { get; set; }
    public string? WeightOfKONeededC { get; set; }
    public string? PhAfterCorrectionA { get; set; }
    public string? PhAfterCorrectionB { get; set; }
    public string? PhAfterCorrectionC { get; set; }
    public int? PhRecordedByIdA { get; set; }
    public int? PhRecordedByIdB { get; set; }
    public int? PhRecordedByIdC { get; set; }
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
    [Column("2PhSpecification")]
    public string? _2PhSpecification { get; set; }
    [Column("2TargetPh")]
    public string? _2TargetPh { get; set; }
    [Column("2PhBeforeCorrection")]
    public string? _2PhBeforeCorrection { get; set; }
    [Column("2PhCorrectionNeeded")]
    public string? _2PhCorrectionNeeded { get; set; }
    [Column("2PhAfterCorrection")]
    public string? _2PhAfterCorrection { get; set; }
    public string? WeightOfSampleDrawnA { get; set; }
    public string? WeightOfSampleDrawnB { get; set; }
    public string? WeightOfSampleDrawnC { get; set; }
    public string? WeightOfKOGUsedA { get; set; }
    public string? WeightOfKOGUsedB { get; set; }
    public string? WeightOfKOGUsedC { get; set; }
    public string? WeightOfFPTankA { get; set; }
    public string? WeightOfFPTankB { get; set; }
    public string? WeightOfFPTankC { get; set; }
    public string? WeightOfKONeededA { get; set; }
    public string? WeightOfKONeededB { get; set; }
    public string? WeightOfKONeededC { get; set; }
    public string? PhAfterCorrectionA { get; set; }
    public string? PhAfterCorrectionB { get; set; }
    public string? PhAfterCorrectionC { get; set; }
    public int? PhRecordedByIdA { get; set; }
    public int? PhRecordedByIdB { get; set; }
    public int? PhRecordedByIdC { get; set; }
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
    public string? FPTankNumber { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrDryingAndDryerLogSheet
{
    public int Id { get; set; }
    public int? BatchID { get; set; }
    public string? OrificeSize { get; set; }
    public string? Core { get; set; }
    public string? LancePosition { get; set; }
    public string? Water { get; set; }
    public string? Product1 { get; set; }
    public string? Product2 { get; set; }
    public string? Product3 { get; set; }
    public string? Product4 { get; set; }
    public string? Product5 { get; set; }
    public string? Product6 { get; set; }
    public string? EndTime { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrBlendSampleTable
{
    public int Id { get; set; }
    public int? BlendID { get; set; }
    public string? SampleNumber { get; set; }
    public string? Test { get; set; }
    public string? Specification { get; set; }
    public string? SpecificationForInformation { get; set; }
    public string? Result { get; set; }
    public string? ResultHYD1 { get; set; }
    public string? ResultHYD2 { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrWsvSlnSampleTable
{
    public int Id { get; set; }
    public int? WSVSlnID { get; set; }
    public string? SampleNumber { get; set; }
    public string? Test { get; set; }
    public string? Specification { get; set; }
    public string? SpecificationForInformation { get; set; }
    public string? Result { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrFptSampleTable
{
    public int Id { get; set; }
    public int? FPTID { get; set; }
    public string? SampleNumber { get; set; }
    public string? Test { get; set; }
    public string? Specification { get; set; }
    public string? SpecificationForInformation { get; set; }
    public string? Result { get; set; }
    public string? Result2 { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrUhtSampleTable
{
    public int Id { get; set; }
    public int? UHTEvaporatorID { get; set; }
    public string? Time { get; set; }
    public string? HMIDensity { get; set; }
    public string? TotalSolids { get; set; }
    public string? Viscosity { get; set; }
    public string? Viscosity2 { get; set; }
    public string? Viscosity4 { get; set; }
    public string? Viscosity5 { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrDryerParameterTable
{
    public int Id { get; set; }
    public int? DryerParameterID { get; set; }
    public int? ParameterID { get; set; }
    public string? TagID { get; set; }
    public string? UOM { get; set; }
    public string? PresetValue { get; set; }
    public string? PreTrialValue { get; set; }
    public string? PostTrialValue { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrDryerParameterParameter
{
    public int Id { get; set; }
    public string? Parameter { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrDryingAndDryerPerformTable
{
    public int Id { get; set; }
    public int? DryingAndDryerLogSheetID { get; set; }
    public string? SampleNo { get; set; }
    public string? Test { get; set; }
    public string? Specification { get; set; }
    public string? BOB { get; set; }
    public string? MOB { get; set; }
    public string? EOB { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrDryingAndDryerLogSheetTable
{
    public int Id { get; set; }
    public int? DryingAndDryerLogSheetID { get; set; }
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public string? Freq { get; set; }
    public string? Product1 { get; set; }
    public string? Product2 { get; set; }
    public string? Product3 { get; set; }
    public string? Product4 { get; set; }
    public string? Product5 { get; set; }
    public string? Product6 { get; set; }
    public bool isActive { get; set; }
}

public sealed class EbrDryingAndDryerCollectSampleTable
{
    public int Id { get; set; }
    public int? DryingAndDryerLogSheetID { get; set; }
    public string? SampleNo { get; set; }
    public string? BulkDensity { get; set; }
    public string? MoistureContent { get; set; }
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
