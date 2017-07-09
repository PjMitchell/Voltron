using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using AggregationMaper.Interfaces;

namespace AggregationMaper
{

    internal class AggregationMapper : Actor, IAggregationMapper
    {
        /// <summary>
        /// Initializes a new instance of AggregationMapper
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public AggregationMapper(ActorService actorService, ActorId actorId)
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

            return Task.FromResult(true);
        }


        Task<Dictionary<int, double>> IAggregationMapper.MapAsync(IEnumerable<Dictionary<int, double>> values, CancellationToken cancellationToken)
        {
            return Task.FromResult(Aggregate(values));
        }

        private Dictionary<int, double> Aggregate(IEnumerable<Dictionary<int, double>> values)
        {
            using (var iterator = values.GetEnumerator())
            {
                if (!iterator.MoveNext())
                    throw new InvalidOperationException("Sequence contains no elements");
                var seed = iterator.Current.ToDictionary(k => k.Key, v => v.Value);
                while(iterator.MoveNext())
                {
                    foreach (var kvp in iterator.Current)
                    {
                        seed[kvp.Key] = seed[kvp.Key] + kvp.Value;
                    }
                }
                return seed;
            }
        }
    }
}
