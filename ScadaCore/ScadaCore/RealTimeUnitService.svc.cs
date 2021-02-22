using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ScadaCore
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RealTimeUnitService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select RealTimeUnitService.svc or RealTimeUnitService.svc.cs at the Solution Explorer and start debugging.
    public class RealTimeUnitService : IRealTimeUnitService
    {
        static readonly object locker = new object();
        public bool initialize(string id, string address)
        {
            if(RealTimeDriver.values.ContainsKey(address))
            {
                return false;
            } else
            {
                lock(locker)
                {
                    RealTimeDriver.values[address] = 0;
                }
                
            }
            return true;
        }

        public void sendValueToAddress(string address, int number)
        {
            lock(locker)
            {
                RealTimeDriver.values[address] = number;
            }
            
        }
    }
}
