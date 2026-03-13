using EduPortal.Domain.Entities.Cms;
using EduPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduPortal.Infrastructure.Persistence.Configurations;

public class CmsBannerConfiguration : IEntityTypeConfiguration<CmsBanner>
{
    public void Configure(EntityTypeBuilder<CmsBanner> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Key).HasMaxLength(100).IsRequired();
        builder.HasIndex(b => b.Key).IsUnique();
        builder.Property(b => b.UpdatedAt).HasColumnType("timestamptz");
    }
}

public class CmsPageConfiguration : IEntityTypeConfiguration<CmsPage>
{
    public void Configure(EntityTypeBuilder<CmsPage> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Slug).HasMaxLength(100).IsRequired();
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.UpdatedAt).HasColumnType("timestamptz");
    }
}

public class CmsFaqConfiguration : IEntityTypeConfiguration<CmsFaq>
{
    public void Configure(EntityTypeBuilder<CmsFaq> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.UpdatedAt).HasColumnType("timestamptz");
    }
}

public class CmsFooterConfiguration : IEntityTypeConfiguration<CmsFooter>
{
    public void Configure(EntityTypeBuilder<CmsFooter> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();
        builder.Property(f => f.SocialLinksJson).HasColumnType("jsonb");
        builder.Property(f => f.ColumnsJson).HasColumnType("jsonb");
        builder.Property(f => f.UpdatedAt).HasColumnType("timestamptz");
    }
}

public class CmsSectionConfiguration : IEntityTypeConfiguration<CmsSection>
{
    public void Configure(EntityTypeBuilder<CmsSection> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Key).HasMaxLength(100).IsRequired();
        builder.HasIndex(s => s.Key).IsUnique();
    }
}

public class CmsSettingConfiguration : IEntityTypeConfiguration<CmsSetting>
{
    public void Configure(EntityTypeBuilder<CmsSetting> builder)
    {
        builder.HasKey(s => s.Key);
        builder.Property(s => s.Key).HasMaxLength(100);
        builder.Property(s => s.DataType).HasConversion<string>();
        builder.Property(s => s.UpdatedAt).HasColumnType("timestamptz");
    }
}

public class CmsFeatureFlagConfiguration : IEntityTypeConfiguration<CmsFeatureFlag>
{
    public void Configure(EntityTypeBuilder<CmsFeatureFlag> builder)
    {
        builder.HasKey(f => f.Key);
        builder.Property(f => f.Key).HasMaxLength(100);
        builder.Property(f => f.UpdatedAt).HasColumnType("timestamptz");
    }
}

public class CmsPromoBannerConfiguration : IEntityTypeConfiguration<CmsPromoBanner>
{
    public void Configure(EntityTypeBuilder<CmsPromoBanner> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.UpdatedAt).HasColumnType("timestamptz");
    }
}
