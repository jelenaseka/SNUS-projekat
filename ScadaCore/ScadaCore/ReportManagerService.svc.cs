using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web.Hosting;

namespace ScadaCore
{
    public class ReportManagerService : IReportManagerService
    {
        public List<AlarmValue> GetAlarmsByDateRange(DateTime dateFrom, DateTime dateTo, string sortBy, string sortType)
        {
            //List<string> alarmsList = new List<string>();

            //List<AlarmValue> alarmsQuery;
            //using (var db = new DatabaseContext())
            //{
            //    if (sortBy == "time" && sortType == "asc")
            //    {
            //        alarmsQuery = db.Alarms.OrderBy(a => a.Time).ToList();
            //    }
            //    else if (sortBy == "time" && sortType == "desc")
            //    {
            //        alarmsQuery = db.Alarms.OrderByDescending(a => a.Time).ToList();
            //    }
            //    else if (sortBy == "priority" && sortType == "asc")
            //    {
            //        alarmsQuery = db.Alarms.OrderBy(a => a.Priority).ToList();
            //    }
            //    else
            //    {
            //        alarmsQuery = db.Alarms.OrderByDescending(a => a.Priority).ToList();
            //    }
            //}
            //foreach (var alarm in alarmsQuery)
            //{
            //    DateTime updatedDate = alarm.Time.AddMilliseconds(-alarm.Time.Millisecond);
            //    if (DateTime.Compare(dateFrom, updatedDate) < 0 && DateTime.Compare(dateTo, updatedDate) > 0 ||
            //        DateTime.Compare(dateFrom, updatedDate) == 0 || DateTime.Compare(dateTo, updatedDate) == 0)
            //    {
            //        alarmsList.Add(GetAlarmString(alarm));
            //    }
            //}

            //return alarmsList;
            using (var db = new DatabaseContext())
            {
                return db.Alarms.OrderBy(a => a.Time).ToList();
            }
        }

        public List<string> GetAlarmsByPriority(int priority, string sortType)
        {
            List<string> alarmsList = new List<string>();
            List<AlarmValue> alarms;

            using (var db = new DatabaseContext())
            {
                alarms = sortType == "asc" ? db.Alarms.Where(a => a.Priority == priority).OrderBy(a => a.Time).ToList() 
                                 : db.Alarms.Where(a => a.Priority == priority).OrderByDescending(a => a.Time).ToList();
            }

            foreach (var alarm in alarms)
                alarmsList.Add(GetAlarmString(alarm));
            
            return alarmsList;
        }

        public List<string> GetTagsByDateRange(DateTime dateFrom, DateTime dateTo, string sortType)
        {
            List<string> tagsList = new List<string>();
            List<TagValue> tagsQuery;

            using (var db = new DatabaseContext())
            {
                tagsQuery = sortType == "asc" ? db.Values.OrderBy(v => v.Time).ToList() : db.Values.OrderByDescending(v => v.Time).ToList();
            }

            foreach (var tag in tagsQuery)
            {
                DateTime updatedDate = tag.Time.AddMilliseconds(-tag.Time.Millisecond);
                if (DateTime.Compare(dateFrom, updatedDate) < 0 && DateTime.Compare(dateTo, updatedDate) > 0 ||
                    DateTime.Compare(dateFrom, updatedDate) == 0 || DateTime.Compare(dateTo, updatedDate) == 0)
                {
                    tagsList.Add(GetTagString(tag));
                }
            }

            return tagsList;
        }

        public List<string> GetAnalogInputs(string sortType)
        {
            return DisplayInputs("analog", sortType);
        }

        public List<string> GetDigitalInputs(string sortType)
        {
            return DisplayInputs("digital", sortType);
        }

        public List<string> GetTagValuesByName(string tagName, string sortType)
        {
            List<string> tagsList = new List<string>();
            List<TagValue> tagValues;

            using (var db = new DatabaseContext())
            {
                tagValues = sortType == "asc" ? db.Values.Where(t => t.TagName == tagName).OrderBy(v => v.Value).ToList()
                                                : db.Values.Where(t => t.TagName == tagName).OrderByDescending(v => v.Value).ToList();
            }

            foreach (var value in tagValues)
            {
                tagsList.Add(GetTagString(value));
            }

            return tagsList;
        }
        private List<string> DisplayInputs(string type, string sortType)
        {
            List<string> analogInputs = new List<string>();
            List<TagValue> inputs;

            using (var db = new DatabaseContext())
            {
                inputs = sortType == "asc" ? db.Values.Where(t => t.Type == type && t.Tag == "input").OrderBy(v => v.Time).ToList()
                                           : db.Values.Where(t => t.Type == type && t.Tag == "input").OrderByDescending(v => v.Time).ToList();
            }

            foreach (var input in inputs)
                analogInputs.Add(GetTagString(input));

            return analogInputs;
        }

        private string GetTagString(TagValue tag)
        {
            return $"Tag name: {tag.TagName}, Tag: {tag.Tag}, Type: {tag.Type}, Value: {tag.Value}, Time: {tag.Time}";
        }

        private string GetAlarmString(AlarmValue alarm)
        {
            return $"Tag name: {alarm.TagName}, Limit: {alarm.Limit}, Trigger value: {alarm.TriggerValue}, Priority: {alarm.Priority}, Time: {alarm.Time}";
        }

       
        
        

    }
}
