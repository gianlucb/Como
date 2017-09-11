using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Como.Communication;
using Como.Model;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Como.Cluster.AgendaManager
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class AgendaManager : StatefulService, IAgendaManager
    {
        public AgendaManager(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {        
            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };
        }


        #region IAgendaManager
        public async Task<Agenda> GetAgendaAsync(string eventId)
        {            
            try
            {
                if (String.IsNullOrEmpty(eventId)) return null;
                Agenda result = null;

                var store = await StateManager.GetOrAddAsync<IReliableDictionary<string, Agenda>>("Agendas").ConfigureAwait(false);

                // Create a new Transaction object for this partition
                using (ITransaction tx = StateManager.CreateTransaction())
                {
                    if (await store.ContainsKeyAsync(tx, eventId))
                    {
                        result = (await store.TryGetValueAsync(tx, eventId)).Value;
                    }
                    await tx.CommitAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Error($"Failed to get the agenda from the dictionary {eventId}: " + ex.Message);
                return null;
            }
        }

        public async Task<bool> CreateOrUpdateAgendaAsync(Agenda agenda)
        {
            //this queues the event creation task and return
            try
            {
                if (agenda == null || String.IsNullOrEmpty(agenda.EventID) == null) return false;

                var store = await StateManager.GetOrAddAsync<IReliableDictionary<string,Agenda>>("Agendas").ConfigureAwait(false);

                // Create a new Transaction object for this partition
                using (ITransaction tx = StateManager.CreateTransaction())
                {
                    await store.AddOrUpdateAsync(tx, agenda.EventID, agenda, (k, a) => agenda);
                    await tx.CommitAsync();
                }
             
                return true;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Error($"Failed to add the agenda to the dictionary {agenda.ID}: " + ex.Message);
                return false;
            }
        
        }

     
        #endregion

    }
}
