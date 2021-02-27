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
        bool Initialize(string id, string address, byte[] signature, string message);

        [OperationContract]
        void SendValueToAddress(string address, int number);
    }
}
