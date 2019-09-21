using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Orleans.Patterns.EventSourcing;
using System;
using System.Diagnostics.Contracts;
using Test.Orleans.Patterns.Contracts;
using Test.Orleans.Patterns.EventSourcing;
using EventS;

namespace Test.Orleans.Patterns.Grains
{

    public class AddingAggregatorGrain : EventAggregatorGrain<Number>, IAddingAggregatorGrain
    {
        public AddingAggregatorGrain(CloudTable eventsTable, ILogger<AddingAggregatorGrain> logger) : base(eventsTable, logger) { }

        protected override Func<(Guid, DateTimeOffset, Number)> InitializeSeed(Number seed) =>
            () => (Guid.Empty, DateTimeOffset.MinValue, seed ?? new Number(0.0));

        protected override (Guid, DateTimeOffset, Number) ProcessEvent((Guid, DateTimeOffset, Number) seed, BusinessEvent curr)
        {
            Contract.Requires(curr != null);

            var (seedId, seedTimestamp, seedPayload) = seed;

            var (id, timestamp) =
                (curr.EventTimestamp > seedTimestamp)
                ? (curr.EventIdentifier, curr.EventTimestamp)
                : (seedId, seedTimestamp);

            switch (curr.BusinessEventEnum)
            {
                default:
                case (int)NumberOperation.Add:
                    {
                        var currPayload = curr.GetValue<Number>();
                        var value = new Number((seedPayload?.Value ?? 0) + (currPayload?.Value ?? 0));

                        return (id, timestamp, value);
                    }
            }
        }
    }

    public class AddingAggregatorGrain2 : EventAggregatorGrain<EventS.Real.Number>, IAddingAggregatorGrainF
    {
        public AddingAggregatorGrain2(CloudTable eventsTable, ILogger<AddingAggregatorGrain2> logger) : base(eventsTable, logger) { }

        protected override Func<(Guid, DateTimeOffset, EventS.Real.Number)> InitializeSeed(EventS.Real.Number seed) =>
            EventS.Real.InitializeSeed(seed);

        protected override (Guid, DateTimeOffset, EventS.Real.Number) ProcessEvent((Guid, DateTimeOffset, EventS.Real.Number) seed, BusinessEvent curr) =>
            EventS.Real.ProcessEvent(seed, curr);
    }
}
