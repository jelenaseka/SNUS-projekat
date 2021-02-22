using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RealTimeUnit
{
    class Program
    {
        static ServiceReference.RealTimeUnitServiceClient rtuClient = new ServiceReference.RealTimeUnitServiceClient();
        static void Main(string[] args)
        {
            Console.WriteLine("Registracija RTU");
            Console.WriteLine("Id: ");
            string id = Console.ReadLine();
            Console.WriteLine("Gornja granica:");
            int highLimit;
            bool success1 = Int32.TryParse(Console.ReadLine(), out highLimit);
            Console.WriteLine("Donja granica:");
            int lowLimit;
            bool success2 = Int32.TryParse(Console.ReadLine(), out lowLimit);
            if(!(success1 && success2))
            {
                Console.WriteLine("Pogresan unos.");
                Console.ReadKey();
                System.Environment.Exit(1);
            }
            Console.WriteLine("Adresa RTD:");
            string address = Console.ReadLine();
            rtuClient.initialize(id, address);
            Random rnd = new Random();
            Console.WriteLine("Started working...");
            while (true)
            {
                
                rtuClient.sendValueToAddress(address,rnd.Next(lowLimit,highLimit));
                Thread.Sleep(5000);
            }
        }
    }
}
