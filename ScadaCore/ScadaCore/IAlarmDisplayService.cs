using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ScadaCore
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IAlarmDisplayService" in both code and config file together.
    [ServiceContract(CallbackContract = typeof(IAlarmDisplayServiceCallback))]
    public interface IAlarmDisplayService
    {
        [OperationContract]
        void initialize();
    }
    public interface IAlarmDisplayServiceCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnAlarmInvoked(string alarm);
    }
}
