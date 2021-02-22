using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ScadaCore
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ITrendingService" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(ITrendingServiceCallback))]
    public interface ITrendingService
    {
        [OperationContract]
        void initialize();
    }
    public interface ITrendingServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnInputTagChange(string input, double value);
    }
}
