using ebr_powerbi.Infrastructure.Data.Ebr;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ebr_powerbi.Infrastructure.Data.Configurations;

public sealed class EbrBatchConfiguration : IEntityTypeConfiguration<EbrBatch>
{
    public void Configure(EntityTypeBuilder<EbrBatch> builder)
    {
        builder.ToTable("Batches");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.FileType)
            .WithMany()
            .HasForeignKey(x => x.FileTypeID);
        builder.HasOne(x => x.PdrsNumber)
            .WithMany()
            .HasForeignKey(x => x.PDRSID);
    }
}

public sealed class EbrFileTypeConfiguration : IEntityTypeConfiguration<EbrFileType>
{
    public void Configure(EntityTypeBuilder<EbrFileType> builder)
    {
        builder.ToTable("FileType");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrCoversheetConfiguration : IEntityTypeConfiguration<EbrCoversheet>
{
    public void Configure(EntityTypeBuilder<EbrCoversheet> builder)
    {
        builder.ToTable("Coversheets");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Batch)
            .WithMany()
            .HasForeignKey(x => x.BatchID);
        builder.HasOne(x => x.FileType)
            .WithMany()
            .HasForeignKey(x => x.FileTypeID);
    }
}

public sealed class EbrCoversheetsWiConfiguration : IEntityTypeConfiguration<EbrCoversheetsWi>
{
    public void Configure(EntityTypeBuilder<EbrCoversheetsWi> builder)
    {
        builder.ToTable("Coversheets_WI");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Batch)
            .WithMany()
            .HasForeignKey(x => x.BatchID);
        builder.HasOne(x => x.FileType)
            .WithMany()
            .HasForeignKey(x => x.FileTypeID);
    }
}

public sealed class EbrPdrsNumberConfiguration : IEntityTypeConfiguration<EbrPdrsNumber>
{
    public void Configure(EntityTypeBuilder<EbrPdrsNumber> builder)
    {
        builder.ToTable("PDRSNumbers");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Location)
            .WithMany()
            .HasForeignKey(x => x.LocationID);
    }
}

public sealed class EbrLocationConfiguration : IEntityTypeConfiguration<EbrLocation>
{
    public void Configure(EntityTypeBuilder<EbrLocation> builder)
    {
        builder.ToTable("Locations");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrPifConfiguration : IEntityTypeConfiguration<EbrPif>
{
    public void Configure(EntityTypeBuilder<EbrPif> builder)
    {
        builder.ToTable("PIF");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrWsmConfiguration : IEntityTypeConfiguration<EbrWsm>
{
    public void Configure(EntityTypeBuilder<EbrWsm> builder)
    {
        builder.ToTable("WSM");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrPiwConfiguration : IEntityTypeConfiguration<EbrPiw>
{
    public void Configure(EntityTypeBuilder<EbrPiw> builder)
    {
        builder.ToTable("PIW");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrPifSampleConfiguration : IEntityTypeConfiguration<EbrPifSample>
{
    public void Configure(EntityTypeBuilder<EbrPifSample> builder)
    {
        builder.ToTable("PIFSampleTable");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrWsmSampleConfiguration : IEntityTypeConfiguration<EbrWsmSample>
{
    public void Configure(EntityTypeBuilder<EbrWsmSample> builder)
    {
        builder.ToTable("WSMSampleTable");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrPiwSampleConfiguration : IEntityTypeConfiguration<EbrPiwSample>
{
    public void Configure(EntityTypeBuilder<EbrPiwSample> builder)
    {
        builder.ToTable("PIWSampleTable");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrBlendConfiguration : IEntityTypeConfiguration<EbrBlend>
{
    public void Configure(EntityTypeBuilder<EbrBlend> builder)
    {
        builder.ToTable("Blend");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrWsvSlnConfiguration : IEntityTypeConfiguration<EbrWsvSln>
{
    public void Configure(EntityTypeBuilder<EbrWsvSln> builder)
    {
        builder.ToTable("WSVSln");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrFptConfiguration : IEntityTypeConfiguration<EbrFpt>
{
    public void Configure(EntityTypeBuilder<EbrFpt> builder)
    {
        builder.ToTable("FPT");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrUhtEvaporatorConfiguration : IEntityTypeConfiguration<EbrUhtEvaporator>
{
    public void Configure(EntityTypeBuilder<EbrUhtEvaporator> builder)
    {
        builder.ToTable("UHTEvaporator");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrEvapParamaterConfiguration : IEntityTypeConfiguration<EbrEvapParamater>
{
    public void Configure(EntityTypeBuilder<EbrEvapParamater> builder)
    {
        builder.ToTable("EvapParamaters");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrEvapLogSheetConfiguration : IEntityTypeConfiguration<EbrEvapLogSheet>
{
    public void Configure(EntityTypeBuilder<EbrEvapLogSheet> builder)
    {
        builder.ToTable("EvapLogSheet");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrDryerParameterConfiguration : IEntityTypeConfiguration<EbrDryerParameter>
{
    public void Configure(EntityTypeBuilder<EbrDryerParameter> builder)
    {
        builder.ToTable("DryerParameters");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrDryingAndDryerLogSheetConfiguration : IEntityTypeConfiguration<EbrDryingAndDryerLogSheet>
{
    public void Configure(EntityTypeBuilder<EbrDryingAndDryerLogSheet> builder)
    {
        builder.ToTable("DryingAndDryerLogSheet");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrBlendSampleTableConfiguration : IEntityTypeConfiguration<EbrBlendSampleTable>
{
    public void Configure(EntityTypeBuilder<EbrBlendSampleTable> builder)
    {
        builder.ToTable("BlendSampleTable");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrWsvSlnSampleTableConfiguration : IEntityTypeConfiguration<EbrWsvSlnSampleTable>
{
    public void Configure(EntityTypeBuilder<EbrWsvSlnSampleTable> builder)
    {
        builder.ToTable("WSVSlnSampleTable");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrFptSampleTableConfiguration : IEntityTypeConfiguration<EbrFptSampleTable>
{
    public void Configure(EntityTypeBuilder<EbrFptSampleTable> builder)
    {
        builder.ToTable("FPTSampleTable");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrUhtSampleTableConfiguration : IEntityTypeConfiguration<EbrUhtSampleTable>
{
    public void Configure(EntityTypeBuilder<EbrUhtSampleTable> builder)
    {
        builder.ToTable("UHTSampleTable");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrDryerParameterTableConfiguration : IEntityTypeConfiguration<EbrDryerParameterTable>
{
    public void Configure(EntityTypeBuilder<EbrDryerParameterTable> builder)
    {
        builder.ToTable("DryerParameterTables");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrDryerParameterParameterConfiguration : IEntityTypeConfiguration<EbrDryerParameterParameter>
{
    public void Configure(EntityTypeBuilder<EbrDryerParameterParameter> builder)
    {
        builder.ToTable("DryerParameterParameter");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrDryingAndDryerPerformTableConfiguration : IEntityTypeConfiguration<EbrDryingAndDryerPerformTable>
{
    public void Configure(EntityTypeBuilder<EbrDryingAndDryerPerformTable> builder)
    {
        builder.ToTable("DryingAndDryerPerformTable");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrDryingAndDryerLogSheetTableConfiguration : IEntityTypeConfiguration<EbrDryingAndDryerLogSheetTable>
{
    public void Configure(EntityTypeBuilder<EbrDryingAndDryerLogSheetTable> builder)
    {
        builder.ToTable("DryingAndDryerLogSheetTable");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrDryingAndDryerCollectSampleTableConfiguration : IEntityTypeConfiguration<EbrDryingAndDryerCollectSampleTable>
{
    public void Configure(EntityTypeBuilder<EbrDryingAndDryerCollectSampleTable> builder)
    {
        builder.ToTable("DryingAndDryerCollectSampleTable");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrPifWiConfiguration : IEntityTypeConfiguration<EbrPifWi>
{
    public void Configure(EntityTypeBuilder<EbrPifWi> builder)
    {
        builder.ToTable("PIFWI");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrPiwWiConfiguration : IEntityTypeConfiguration<EbrPiwWi>
{
    public void Configure(EntityTypeBuilder<EbrPiwWi> builder)
    {
        builder.ToTable("PIWWI");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrWsmWiConfiguration : IEntityTypeConfiguration<EbrWsmWi>
{
    public void Configure(EntityTypeBuilder<EbrWsmWi> builder)
    {
        builder.ToTable("WSMWI");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrHydrationWiConfiguration : IEntityTypeConfiguration<EbrHydrationWi>
{
    public void Configure(EntityTypeBuilder<EbrHydrationWi> builder)
    {
        builder.ToTable("HydrationWI");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrFptWiConfiguration : IEntityTypeConfiguration<EbrFptWi>
{
    public void Configure(EntityTypeBuilder<EbrFptWi> builder)
    {
        builder.ToTable("FPTWI");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrEvaporatorWiConfiguration : IEntityTypeConfiguration<EbrEvaporatorWi>
{
    public void Configure(EntityTypeBuilder<EbrEvaporatorWi> builder)
    {
        builder.ToTable("EvaporatorWI");
        builder.HasKey(x => x.Id);
    }
}

public sealed class EbrDryerWiConfiguration : IEntityTypeConfiguration<EbrDryerWi>
{
    public void Configure(EntityTypeBuilder<EbrDryerWi> builder)
    {
        builder.ToTable("DryerWI");
        builder.HasKey(x => x.Id);
    }
}
