using Discord.Interactions;

namespace PallasBot.Domain.Exceptions;

public class InteractionFailedException : Exception
{
    public InteractionFailedException(IResult result) : base($"Error type: {result.Error.ToString() ?? "Unknown"}. Reason: {result.ErrorReason}")
    {
    }
}
