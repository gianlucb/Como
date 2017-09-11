using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Como.Model;
using Como.SDK;
using System.Threading;


namespace Como.Extra.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri frontEndUri = new Uri("http://localhost:80");
            ComoClusterFrontEndAPIs client = new ComoClusterFrontEndAPIs();
            client.BaseUri = frontEndUri;

            do
            {
                AddEvent(client);
            } while (true);

        }

        private static void AddAgenda(ComoClusterFrontEndAPIs client, CustomEvent evt)
        {
            Agenda testAgenda = new Agenda(evt.ID);

            for (int i = 0; i < 20; i++)
            {
                Session s = new Session() { ID="S"+i, Title = "Awesome session", Level = SessionLevel.L100, Room = "Room"+i, Abstract = "Abstract", Duration = 45 };
                testAgenda.Sessions.Add(s);
            }

            var result = client.V1AgendaPut(testAgenda);
                
            Console.WriteLine("New Agenda added: " + result);
            Console.WriteLine("-----------------------------------");

            Console.ReadKey();
        }


        private static void AddEvent(ComoClusterFrontEndAPIs client)
        {
            Console.WriteLine("");  
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Event name: ");
            var name = Console.ReadLine();
            var evt = new CustomEvent() { ID = name, Name = "Simple Evt", StartDate = DateTime.Now };
            string result = client.V1EventsPut(evt);
            if (result != null && Uri.IsWellFormedUriString(result,UriKind.RelativeOrAbsolute))
            {
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("New Event added: " + result);
                Console.WriteLine("wait for a while, it is building..");
                AddAgenda(client, evt);
            }
            else
                Console.WriteLine("Error, check the cluster tracing");
            Console.ReadKey();
        }
    }
}
