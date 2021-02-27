using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ScadaCore
{
    [ServiceContract]
    public interface IDatabaseManagerService
    {
        [OperationContract]
        bool IsDatabaseEmpty();
        [OperationContract]
        string ChangeOutputValue(string tagName, double value);

        [OperationContract]
        Dictionary<string, double> GetOutputValues();

        [OperationContract]
        string TurnScanOnOff(string tagName);
        
        [OperationContract]
        bool Registration(string username, string password, string role);
        
        [OperationContract]
        string SignIn(string username, string password);

        [OperationContract]
        string AddOutputTag(string tagName, string desc, string address, double initVal, double lowLimit, double highLimit, string type);

        [OperationContract]
        string AddInputTag(string tagName, string desc, string address, string driver, int scanTime, bool onOffScan, double lowLimit, double highLimit, string type);
        
        [OperationContract]
        string RemoveInputTag(string tagName);

        [OperationContract]
        string RemoveOutputTag(string tagName);
        [OperationContract]
        string AddAlarm(string tagName, string type, double limit, int priority);
        [OperationContract]
        string DeleteAlarm(string tagName);
    }
}
