﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Como.Model;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Como.Communication
{
    public interface IAgendaManager : IService
    {
       Task<bool> CreateOrUpdateAgendaAsync(Agenda agenda);
       Task<Agenda> GetAgendaAsync(string eventId);
    }
}
