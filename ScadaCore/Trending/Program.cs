using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Trending.ServiceReference;

namespace Trending
{
    public class TrendingCallback : ITrendingServiceCallback
    {
        public void OnInputTagChange(string input, double value)
        {
            Console.WriteLine("Tag name: " + input + ", Value: " + value);
        }
    }
    class Program
    {
        static InstanceContext ic = new InstanceContext(new TrendingCallback());
        static TrendingServiceClient trendingClient = new TrendingServiceClient(ic);
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            trendingClient.initialize();
            Console.ReadKey();
        }
    }
}
