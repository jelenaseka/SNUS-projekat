using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ScadaCore
{
    [ServiceContract]
    public interface IRealTimeUnitService
    {
        [OperationContract]
        bool initialize(string id, string address);

        [OperationContract]
        void sendValueToAddress(string address, int number);
    }
}
