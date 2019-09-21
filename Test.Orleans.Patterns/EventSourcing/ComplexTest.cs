using System;
using System.Threading.Tasks;
using Orleans.Testing.Utilities;
using Orleans.TestingHost;
using Orleans.Patterns.EventSourcing;
using Xunit;
using Test.Orleans.Patterns.Contracts;
using Test.Orleans.Patterns.Grains;
using EventS;

namespace Test.Orleans.Patterns.EventSourcing
{
    [Collection(ClusterCollection.Name)]
    public class ComplexTests
    {
        private readonly TestCluster _cluster;
        public ComplexTests(ClusterFixture fixture) =>
            _cluster = fixture?.Cluster ?? throw new ArgumentNullException(nameof(fixture));

        [Fact]
        public async Task New_Version_Aggregator_Processes_Old_Version_Events__Upgrade_Scenario()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            for (var i = 1; i <= 100; i++)
            {
                await g.RecordEventPayload(19999, EventS.Real.Number.NewReal(i)).ConfigureAwait(false);
            }

            var firstVersionAdder = await g.RegisterAggregateGrain<IAddingAggregatorGrainF>().ConfigureAwait(false);
            var firstVersionResult = await firstVersionAdder.GetValue<EventS.Real.Number>().ConfigureAwait(false);

            var nextVersionAdder = await g.RegisterAggregateGrain<IComplexAddingAggregatorGrainF>().ConfigureAwait(false);
            var nextVersionResult = await nextVersionAdder.GetValue<EventS.Complex.Number>().ConfigureAwait(false);

            Assert.Equal(5050, firstVersionResult.Item);
            Assert.Equal(firstVersionResult.Item, nextVersionResult.RealCompoment);
            Assert.Equal(0, nextVersionResult.ImaginaryComponent);
        }

        [Fact]
        // Test that new events do not alter the old aggregates!
        public async Task Old_Version_Aggregator_Processes_New_Version_Events__Rollback_Scenario__REAL__STABLE()
        {
            var primaryKey = Guid.NewGuid();
            var g = _cluster.GrainFactory.GetGrain<IEventSourcedGrain>(primaryKey);

            // old events
            // the system did not now about complex
            for (var i = 1; i <= 100; i++)
            {
                await g.RecordEventPayload(19999, EventS.Real.Number.NewReal(i)).ConfigureAwait(false);
            }
            // aggreagation!
            var firstVersionAdder = await g.RegisterAggregateGrain<IAddingAggregatorGrainF>().ConfigureAwait(false);
            var firstVersionResult = await firstVersionAdder.GetValue<EventS.Real.Number>().ConfigureAwait(false);
            Assert.Equal(5050, firstVersionResult.Item);


            // upgrade system to complex number
            for (var i = 1; i <= 100; i++)
            {
                double asd = (double)i;
                System.Tuple<double, double> test = Tuple.Create(asd, 0.0);
                await g.RecordEventPayload(5, EventS.Complex.Number.NewComplex(test)).ConfigureAwait(false);
            }

            /// the version we deploy of complex knows how to handle old events
            var nextVersionAdder = await g.RegisterAggregateGrain<IComplexAddingAggregatorGrainF>().ConfigureAwait(false);
            var nextVersionResult = await nextVersionAdder.GetValue<EventS.Complex.Number>().ConfigureAwait(false);
            Assert.Equal(10100, nextVersionResult.RealCompoment);
            Assert.Equal(0, nextVersionResult.ImaginaryComponent);

            // expect the old aggregate to be stable
            var firstVersionNewResult = await firstVersionAdder.GetValue<EventS.Real.Number>().ConfigureAwait(false);
            Assert.Equal(firstVersionNewResult.Item, firstVersionResult.Item);


        }
    }
}