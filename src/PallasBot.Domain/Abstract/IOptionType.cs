using Microsoft.Extensions.Configuration;

namespace PallasBot.Domain.Abstract;

public interface IOptionType<out T>
{
    public static abstract T Get(IConfiguration configuration);
}
