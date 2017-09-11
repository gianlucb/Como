using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Newtonsoft.Json;

namespace Como.Model
{
    public class Agenda
    {
        public Agenda() { ID = ""; Sessions = new List<Session>(); }
        public Agenda(string eventID) : this() { EventID = eventID; }
        public String EventID { get; set; }     
        public String ID { get; set; }
        public List<Session> Sessions { get; set; }
    }

    public enum SessionLevel : int
    {
        L100 = 100,
        L200 = 200,
        L300 = 300,
        L400 = 400
    }

    public class Session
    {
        public Session() { ID = ""; Speakers = new List<String>(); }
        public String ID { get; set; }
        public String Title { get; set; }
        public String Abstract { get; set; }
        public List<String> Speakers { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; } //minutes

        [JsonIgnore]
        public DateTime StopTime => StartTime.Add(new TimeSpan(0,Duration,0));  
        public SessionLevel Level { get; set; }
        public String Room { get; set; }      
    }

    
}
