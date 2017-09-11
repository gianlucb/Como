using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Como.Model;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Como.Communication
{
    public interface IEventManager : IService
    {
        /// <summary>
        /// Queue the creation of a new Event service. The creation task is added to a queue and ran asynchronously later. If the event already exists will be updated on DB, no double insertions
        /// </summary>
        /// <returns>the Uri where the website will be live when created</returns>
       Task<Uri> CreateOrUpdateEventAsync(CustomEvent evt);

    }
}
