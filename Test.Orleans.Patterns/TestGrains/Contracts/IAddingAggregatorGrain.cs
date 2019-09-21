using Orleans.Patterns.EventSourcing;

namespace Test.Orleans.Patterns.Contracts
{
    public interface IAddingAggregatorGrain : IEventAggregatorGrain { }
    public interface IAddingAggregatorGrainF : IEventAggregatorGrain { }
    public interface IComplexAddingAggregatorGrainF : IEventAggregatorGrain { }

}
