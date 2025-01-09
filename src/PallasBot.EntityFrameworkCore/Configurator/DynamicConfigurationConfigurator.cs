using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PallasBot.Domain.Entities;
using PallasBot.Domain.Enums;

namespace PallasBot.EntityFrameworkCore.Configurator;

public class DynamicConfigurationConfigurator : IEntityTypeConfiguration<DynamicConfiguration>
{
    public void Configure(EntityTypeBuilder<DynamicConfiguration> builder)
    {
        builder.HasKey(x => new { x.GuildId, x.Key });

        builder.Property(x => x.Key)
            .HasConversion<EnumToStringConverter<DynamicConfigurationKey>>();
    }
}
