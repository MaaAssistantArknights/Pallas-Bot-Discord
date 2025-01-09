using Microsoft.EntityFrameworkCore;
using PallasBot.Domain.Abstract;
using PallasBot.Domain.Entities;
using PallasBot.Domain.Enums;

namespace PallasBot.EntityFrameworkCore.Services;

public class DynamicConfigurationService : IDynamicConfigurationService
{
    private readonly PallasBotDbContext _pallasBotDbContext;

    public DynamicConfigurationService(PallasBotDbContext pallasBotDbContext)
    {
        _pallasBotDbContext = pallasBotDbContext;
    }

    public async Task<string?> GetByGuildAsync(ulong guildId, DynamicConfigurationKey key)
    {
        return await _pallasBotDbContext.DynamicConfigurations
            .Where(x => x.GuildId == guildId && x.Key == key)
            .Select(x => x.Value)
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<ulong, string>> GetAllAsync(DynamicConfigurationKey key)
    {
        var configs = await _pallasBotDbContext.DynamicConfigurations
            .Where(x => x.Key == key)
            .ToListAsync();
        return configs
            .ToDictionary(x => x.GuildId, x => x.Value);
    }

    public async Task SetAsync(ulong guildId, DynamicConfigurationKey key, string value)
    {
        var existing = _pallasBotDbContext.DynamicConfigurations
            .FirstOrDefault(x => x.GuildId == guildId && x.Key == key);
        if (existing is not null)
        {
            existing.Value = value;
            _pallasBotDbContext.DynamicConfigurations.Update(existing);
        }
        else
        {
            await _pallasBotDbContext.DynamicConfigurations.AddAsync(new DynamicConfiguration
            {
                GuildId = guildId,
                Key = key,
                Value = value
            });
        }

        await _pallasBotDbContext.SaveChangesAsync();
    }
}
