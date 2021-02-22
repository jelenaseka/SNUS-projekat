using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ScadaCore
{
    public class Alarm
    {
        [Key]
        public string Id { get; set; }
        public string Type { get; set; }
        public int Priority { get; set; }
        public DateTime Time { get; set; }
        public string TagName { get; set; }
        public double Limit { get; set; }
        public Alarm() { }
        public Alarm(string id, string type, int priority, DateTime time, string tagName, double limit)
        {
            Id = id;
            Type = type;
            Priority = priority;
            Time = time;
            TagName = tagName;
            Limit = limit;
        }
    }
}