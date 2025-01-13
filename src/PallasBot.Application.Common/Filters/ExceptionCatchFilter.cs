using System.Diagnostics;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace PallasBot.Application.Common.Filters;

public class ExceptionCatchFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<ExceptionCatchFilter<T>> _logger;

    public ExceptionCatchFilter(ILogger<ExceptionCatchFilter<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        try
        {
            await next.Send(context);
        }
        catch (Exception e)
        {
            var name = context.Message.GetType().Name;
            var messageId = context.MessageId;

            _logger.LogError(e, "Error processing message {MessageName} with id {MessageId}", name, messageId);

            Activity.Current?.AddTag("error_message_type", name);
            Activity.Current?.AddTag("error_message_id", messageId);

            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
    }
}
