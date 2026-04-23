using Serilog.Core;
using Serilog.Events;

namespace MotorCare.Api.Logging;

public sealed class EventNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue("EventId", out var eventIdValue) || eventIdValue is not StructureValue structureValue)
        {
            return;
        }

        var nameProperty = structureValue.Properties.FirstOrDefault(x => x.Name == "Name");
        if (nameProperty is null || nameProperty.Value is not ScalarValue { Value: string eventName } || string.IsNullOrWhiteSpace(eventName))
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("EventName", eventName));
    }
}
