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
        List<string> DisplayAlarmsByDate(DateTime dateFrom, DateTime dateTo, string sortBy, string sortType);

        [OperationContract]
        List<string> DisplayAlarmsByPriority(int priority, string sortType);

        [OperationContract]
        List<string> DisplayTagsByDate(DateTime dateFrom, DateTime dateTo, string sortType);

        [OperationContract]
        List<string> DisplayAnalogInputs(string sortType);

        [OperationContract]
        List<string> DisplayDigitalInputs(string sortType);

        [OperationContract]
        List<string> DisplayTagById(string tagName, string sortType);
    }
}
