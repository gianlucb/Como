using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Como.Communication;
using Como.Model;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Services.Client;
using Como.Cluster.FrontEnd;

namespace Como.Cluster.FrontEnd.Controllers
{
    [Route("v1/Events")]
    public class EventsController : Controller
    {
        private IConfiguration _configuration;

        public EventsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPut]
        public Uri Put([FromBody]CustomEvent evt)
        {
            if (evt == null && String.IsNullOrEmpty(evt.ID))
                return null;
            var eventManager = ServiceProxy.Create<IEventManager>(_configuration.GetValue<Uri>("ComoConfig:EventManagerUri"), new ServicePartitionKey(0));
            var createdEvt = eventManager.CreateOrUpdateEventAsync(evt);
            if (createdEvt.Result == null) BadRequest();
            return createdEvt.Result;
        }
      
    }
}
