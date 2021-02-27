using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScadaCore
{
    public class Output
    {
        public string Id { get; set; }
        public string TagName { get; set; }
        public string Description { get; set; }
        public string IOAddress { get; set; }
        public double InitialValue { get; set; }
        public string Type { get; set; }
        public Output() { }
        public Output(string tagName, string description, string ioAddress, double initialValue, string type)
        {
            TagName = tagName;
            Description = description;
            IOAddress = ioAddress;
            InitialValue = initialValue;
            Type = type;
        }
    }
    public class DigitalOutput : Output
    {
        public DigitalOutput(string tagName, string description, string ioAddress, double initialValue, string type)
            :base(tagName, description, ioAddress, initialValue, type)
        {
            
        }
    }
    public class AnalogOutput : Output
    {
        public double LowLimit { get; set; }
        public double HighLimit { get; set; }
        public AnalogOutput() { }
        public AnalogOutput(double lowLimit, double highLimit, string tagName, string description, string ioAddress, double initialValue, string type) : base(tagName, description, ioAddress, initialValue, type)
        {
            LowLimit = lowLimit;
            HighLimit = highLimit;
        }
    }
}