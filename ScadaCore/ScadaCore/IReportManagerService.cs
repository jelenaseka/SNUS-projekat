using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ScadaCore
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IReportManagerService" in both code and config file together.
    [ServiceContract]
    public interface IReportManagerService
    {
        [OperationContract]
        List<AlarmValue> GetAlarmsByDateRange(DateTime dateFrom, DateTime dateTo, string sortBy, string sortType);

        [OperationContract]
        List<string> GetAlarmsByPriority(int priority, string sortType);

        [OperationContract]
        List<string> GetTagsByDateRange(DateTime dateFrom, DateTime dateTo, string sortType);

        [OperationContract]
        List<string> GetAnalogInputs(string sortType);

        [OperationContract]
        List<string> GetDigitalInputs(string sortType);

        [OperationContract]
        List<string> GetTagValuesByName(string tagName, string sortType);
    }
}
