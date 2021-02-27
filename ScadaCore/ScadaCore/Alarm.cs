using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ScadaCore
{
    public class Alarm
    {
        
        public string Type { get; set; }
        public int Priority { get; set; }
        
        public string TagName { get; set; }
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

    public class AlarmValue : Alarm
    {
        [Key]
        public int Id { get; set; }
        public DateTime Time { get; set; }
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