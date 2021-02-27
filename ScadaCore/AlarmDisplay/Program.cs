using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AlarmDisplay.ServiceReference;

namespace AlarmDisplay
{
    public class AlarmDisplayCallback : IAlarmDisplayServiceCallback
    {
        public void OnAlarmInvoked(string alarm, int priority)
        {
            for(int i = 0; i < priority; i++)
            {
                Console.WriteLine(alarm);
            }
            Console.WriteLine("-------------------------------------------------------------------------------------------------------");
        }
        
    }
    class Program
    {
        static InstanceContext ic = new InstanceContext(new AlarmDisplayCallback());
        static AlarmDisplayServiceClient alarmClient = new AlarmDisplayServiceClient(ic);
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            alarmClient.initialize();
            Console.ReadKey();
        }
    }
}
