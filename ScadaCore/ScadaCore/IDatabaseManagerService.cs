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
        bool ChangeOutputValue(string tagName, double value);

        [OperationContract]
        Dictionary<string, double> GetOutputValues();

        [OperationContract]
        bool TurnScanOnOff(string tagName, bool scan);
        
        [OperationContract]
        bool Registration(string username, string password, string role);
        
        [OperationContract]
        string SignIn(string username, string password);

        [OperationContract]
        bool AddOutputTag(string tagName, string desc, string address, double initVal, int lowLimit, int highLimit, string type);

        [OperationContract]
        bool AddInputTag(string tagName, string desc, string address, string driver, int scanTime, bool onOffScan, int lowLimit, int highLimit, string type);
        
        [OperationContract]
        bool RemoveInputTag(string tagName);

        [OperationContract]
        bool RemoveOutputTag(string tagName);
        //[OperationContract]
        //void addAlarm(string tagName, string type, double limit, int priority, DateTime time);
        //[OperationContract]
        //void deleteAlarm(string tagName);
    }
}
