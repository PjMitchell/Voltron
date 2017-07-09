using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using AggregationReducer.Interfaces;

namespace AggregationReducer
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class AggregationReducer : Actor, IAggregationReducer
    {
        private const string stateName = "result";
        
        /// <summary>
        /// Initializes a new instance of AggregationReducer
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public AggregationReducer(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            return this.StateManager.TryAddStateAsync("count", 0);
        }

        Task<Dictionary<int, double>> IAggregationReducer.GetResultAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<Dictionary<int, double>>(stateName, cancellationToken);
        }
        Task IAggregationReducer.ReduceAsync(Dictionary<int, double> subResult, CancellationToken cancellationToken)
        {
            return this.StateManager.AddOrUpdateStateAsync(stateName, subResult, (key, value) => Aggregate(value, subResult), cancellationToken);
        }

        private Dictionary<int, double> Aggregate(Dictionary<int, double> valueOne, Dictionary<int, double> valueTwo)
        {
            var result = new Dictionary<int, double>(valueOne.Count);
            foreach (var kvp in valueOne)
            {
                result[kvp.Key] = kvp.Value + valueTwo[kvp.Key];
            }
            return result;
        }
    }
}
