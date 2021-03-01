using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ScadaCore
{
    [DataContract]
    public class Alarm
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public int Priority { get; set; }
        [DataMember]
        public string TagName { get; set; }
        [DataMember]
        public double Limit { get; set; }
        public Alarm() { }
        public Alarm(string type, int priority, string tagName, double limit)
        {
            Type = type;
            Priority = priority;
            TagName = tagName;
            Limit = limit;
        }
    }

    [DataContract]
    public class AlarmValue : Alarm
    {
        [Key]
        public int Id { get; set; }
        [DataMember]
        public DateTime Time { get; set; }
        [DataMember]
        public double TriggerValue { get; set; }
        public AlarmValue() { }
        public AlarmValue(string type, int priority, string tagName, double limit, DateTime time, double triggerValue) 
            : base(type, priority, tagName, limit)
        {
            Time = time;
            TriggerValue = triggerValue;
        }
    }
}