using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Como.Communication;
using Como.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace Como.Cluster.FrontEnd.Controllers
{
    [Produces("application/json")]
    [Route("v1/Agenda")]
    public class AgendaController : Controller
    {
        private IConfiguration _configuration;

        public AgendaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPut]
        public bool Put([FromBody]Agenda agenda)
        {
            var agendaManager = ServiceProxy.Create<IAgendaManager>(_configuration.GetValue<Uri>("ComoConfig:AgendaManagerUri"), new ServicePartitionKey(0));
            var result = agendaManager.CreateOrUpdateAgendaAsync(agenda);
            return result.Result;
        }

        [HttpGet]
        public Agenda Get(string eventId)
        {
            try
            {
                var agendaManager = ServiceProxy.Create<IAgendaManager>(_configuration.GetValue<Uri>("ComoConfig:AgendaManagerUri"), new ServicePartitionKey(0));
                var result = agendaManager.GetAgendaAsync(eventId);
                return result.Result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}