using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ML.Domain.Enums;

namespace ML.Infrastructure.Data.Configurations;

public class SearchHistoryConfiguration : IEntityTypeConfiguration<SearchHistories>
{
    public void Configure(EntityTypeBuilder<SearchHistories> builder)
    {
        builder.ToTable("SearchHistory");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();
        builder.Property(e => e.SearchId)
            .IsRequired()
            .HasMaxLength(50);
        builder.Property(e => e.SearchType)
            .IsRequired()
            .HasConversion<string>();
        builder.Property(e => e.Title).HasMaxLength(200);
        builder.Property(e => e.Url).HasMaxLength(256);
        builder.Property(e => e.Creator).HasMaxLength(200);
        builder.Property(e => e.License).HasMaxLength(200);
        builder.Property(e => e.Provider).HasMaxLength(200);
        builder.Property(e => e.Attribuition).HasMaxLength(1000);
        builder.Property(e => e.RelatedUrl).HasMaxLength(256);
        //builder.Property(e => e.IndexedOn);
        builder.Property(e => e.ForeignLandingUrl).HasMaxLength(256);
        builder.Property(e => e.CreatorUrl).HasMaxLength(256);
        builder.Property(e => e.LicenseVersion).HasMaxLength(200);
        builder.Property(e => e.LicenseUrl).HasMaxLength(256);
        builder.Property(e => e.Source).HasMaxLength(200);
        builder.Property(e => e.Category).HasMaxLength(200);
        builder.Property(e => e.Genres).HasMaxLength(256);
        //builder.Property(e => e.FileSize);
        builder.Property(e => e.FileType).HasMaxLength(50);
        builder.Property(e => e.ThumbNail).HasMaxLength(256);
        builder.Property(e => e.Title).HasMaxLength(200);
        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(StatusEnum.Active);
        builder.Property(e => e.Created)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.LastModified)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.SearchId);
        builder.HasIndex(e => new { e.UserId, e.SearchId});
    }
}
