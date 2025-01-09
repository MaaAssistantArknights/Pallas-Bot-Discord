using PallasBot.Domain.Enums;

namespace PallasBot.Domain.Abstract;

public interface IDynamicConfigurationService
{
    public Task<string?> GetByGuildAsync(ulong guildId, DynamicConfigurationKey key);

    public Task<Dictionary<ulong, string>> GetAllAsync(DynamicConfigurationKey key);

    public Task SetAsync(ulong guildId, DynamicConfigurationKey key, string value);
}
