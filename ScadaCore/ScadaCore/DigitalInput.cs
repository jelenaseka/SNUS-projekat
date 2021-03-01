using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ScadaCore
{
    public class Input
    {
        public string TagName { get; set; }
        public string Description { get; set; }
        public string Driver { get; set; }
        public string IOAddress { get; set; }
        public int ScanTime { get; set; }
        public bool OnOffScan { get; set; }
        public string Type { get; set; }
        
        public Input() { }
        public Input(string tagName, string description, string driver, string ioAddress, int scanTime, bool onOffScan, string type)
        {
            TagName = tagName;
            Description = description;
            Driver = driver;
            IOAddress = ioAddress;
            ScanTime = scanTime;
            OnOffScan = onOffScan;
            Type = type;
        }
    }
    public class DigitalInput : Input
    {
        public DigitalInput(string tagName, string description, string driver, string ioAddress, int scanTime, bool onOffScan, string type)
            :base(tagName, description, driver, ioAddress, scanTime, onOffScan, type)
        {

        }
    }

    public class AnalogInput : Input
    {
        public double LowLimit { get; set; }
        public double HighLimit { get; set; }
        public AnalogInput() { }
        public AnalogInput(double lowLimit, double highLimit, string tagName, string description, string driver, string ioAddress, int scanTime, bool onOffScan, string type)
            : base(tagName, description, driver, ioAddress, scanTime, onOffScan, type)
        {
            LowLimit = lowLimit;
            HighLimit = highLimit;
        }
    }

    public class TagValue
    {
        [Key]
        public int Id { get; set; }
        public string TagName { get; set; }
        public double Value { get; set; }
        public string Type { get; set; }
        public string Tag { get; set; }
        public DateTime Time { get; set; }
    }
}