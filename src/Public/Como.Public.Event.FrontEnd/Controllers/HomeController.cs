using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Como.SDK;
using Microsoft.AspNetCore.Mvc;

namespace Como.Public.Event.FrontEnd.Controllers
{
    public class HomeController : Controller
    {

        StatelessServiceContext _serviceContext;
        private string EventID = "";

        public HomeController(StatelessServiceContext serviceContext)
        {
            //accessing the Service Fabric context
            _serviceContext = serviceContext;
            var configurationPackage = _serviceContext.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            EventID = configurationPackage.Settings.Sections["EventConfig"].Parameters["EventID"].Value;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Message"] = EventID;

            Uri frontEndUri = new Uri("http://localhost:80");
            ComoClusterFrontEndAPIs client = new ComoClusterFrontEndAPIs();
            client.BaseUri = frontEndUri;

            var agenda = await client.V1AgendaGetAsync(EventID);
            ViewBag.Agenda = agenda;

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
