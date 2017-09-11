using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Como.Communication;
using Como.Model;

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using SFHelper;

namespace Como.Cluster.EventManager
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class EventManager : StatefulService, IEventManager
    {

        private readonly string EVENT_APPLICATIONTYPE_NAME = "Como.Public.EventType";
        private readonly string EVENT_APPLICATIONTYPE_VERSION = "1.0.0";
        private readonly string EVENT_DEPLOYMENT_BASEURI = "fabric:/Events/";
        private readonly string EVENT_DEPLOYMENT_NODETYPE = "NodeType2"; //default for local dev cluster (not for online ones)
     
        public EventManager(StatefulServiceContext context) : base(context)
        {

        }

        #region IEventManager
        public async Task<Uri> CreateOrUpdateEventAsync(CustomEvent evt)
        {
            //this queues the event creation task and return
            try
            {
                if (evt == null) return null;

                var store = await StateManager.GetOrAddAsync<IReliableQueue<CustomEvent>>("EventsQueue").ConfigureAwait(false);
               
                // Create a new Transaction object for this partition
                using (ITransaction tx = base.StateManager.CreateTransaction())
                {
                    await store.EnqueueAsync(tx, evt);
                    await tx.CommitAsync();
                }

                //get the listening address

                //each dynamically created service will be listening on a dynamic port. But can be accessed by users via the reverse proxy:
                //ie: http://clustername:19081/Events/XXXXXX/FrontEnd
                //the port of the ReverseProxy is set by the cluster config, can be different for each node type
                //to resolve we need to parse the cluster manifest and then check the value of HttpApplicationGatewayEndpoint 
                //this is done by the Nuget Package SFHelper

                //on dev local machine is already enabled, on production must be enable as described here, or via azure portal
                //https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-reverseproxy

                //the Event applications will be deployed on a specific node type, as defined by their manifest
                //      <PlacementConstraints>
                //          NodeType == NodeType2
                //       </PlacementConstraints >

               
                var config = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
                string fqdn = config.Settings.Sections["ComoConfig"].Parameters["PublicClusterFQDN"].Value;

                Uri listeningUrl = ReverseProxyResolver.GetReverseProxyListeningUrl(fqdn,EVENT_DEPLOYMENT_NODETYPE);

                Uri eventDeploymentUri = new Uri($"{listeningUrl.AbsoluteUri}/Events/" +evt.ID.ToString().ToUpperInvariant()+"/FrontEnd/");          
                ServiceEventSource.Current.Message($"Event {evt.ID.ToString().ToUpperInvariant()} added to the queue. Not ready yet, will be listening at: {eventDeploymentUri}");

                return eventDeploymentUri; //not ready yet
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Error("Create Event failed {evt.ID}: " + ex.Message);
            }
            return null;
        }

        #endregion

        public async Task<bool> BuildEventAsync(CustomEvent evt)
        {
            try
            {
                Uri eventDeploymentUri = new Uri(EVENT_DEPLOYMENT_BASEURI + evt.ID.ToString().ToUpperInvariant());
                FabricClient fabricClient = new FabricClient();
                
                System.Fabric.Query.ApplicationList applicationList = await fabricClient.QueryManager.GetApplicationListAsync(eventDeploymentUri);

                if (applicationList.Count > 0) return false; //already exists              
                
                //passing parameters to the new created application
                NameValueCollection appParameters = new NameValueCollection();
                appParameters.Add("EventID", evt.ID.ToString().ToUpperInvariant());
                ApplicationDescription applicationDesc = new ApplicationDescription(eventDeploymentUri, EVENT_APPLICATIONTYPE_NAME, EVENT_APPLICATIONTYPE_VERSION, appParameters);
                
                await fabricClient.ApplicationManager.CreateApplicationAsync(applicationDesc);

                ServiceEventSource.Current.NewEventCreated(evt.ID.ToString(), eventDeploymentUri.AbsoluteUri);

                return true;
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Error($"Create Event failed {evt.ID}: " + ex.Message);
            }

            return false;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            //FabricTelemetryInitializerExtension.SetServiceCallContext(this.Context);

            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };

        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            int retryCount = 3;
            var store = await StateManager.GetOrAddAsync<IReliableQueue<CustomEvent>>("EventsQueue").ConfigureAwait(false);
            while (!cancellationToken.IsCancellationRequested)
            {
                using (var tx = StateManager.CreateTransaction())
                {                    

                    var itemFromQueue = await store.TryDequeueAsync(tx).ConfigureAwait(false);
                    if (!itemFromQueue.HasValue)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    ServiceEventSource.Current.Message($"Found an event in the queue, ready to be created: {itemFromQueue.Value.ID}");

                    bool creationResult = await BuildEventAsync(itemFromQueue.Value);
                    if (creationResult)
                        await tx.CommitAsync();
                    else
                        if (retryCount-- > 0)
                             tx.Abort(); //try again
                    else
                    {
                        await tx.CommitAsync(); //remove the message from the queue (can also be sent to another queue for later processing)
                        ServiceEventSource.Current.Error($"The event {itemFromQueue.Value.ID} cannot be created.");

                    }
                }
            }
        }
    }
}
