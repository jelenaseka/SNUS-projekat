using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

namespace ScadaCore
{
    public static class TagProcessing
    {
        public delegate void ChangeInputTagDelegate(string input, double value);
        public static event ChangeInputTagDelegate inputChanged;
        public delegate void AlarmCalledDelegate(string alarm, int priority);
        public static event AlarmCalledDelegate alarmCalled;
        public static Dictionary<string, Thread> inputTagThreads = new Dictionary<string, Thread>();
        public static Dictionary<string, Input> inputTags = new Dictionary<string, Input>();
        public static Dictionary<string, Output> outputTags = new Dictionary<string, Output>();
        public static Dictionary<string, double> outputValues = new Dictionary<string, double>();
        public static Dictionary<string, Alarm> alarms = new Dictionary<string, Alarm>();
        public static DatabaseContext databaseContext = new DatabaseContext();
        public static readonly object locker = new object();

        public static void StartThreads()
        {
            foreach (KeyValuePair<string, Thread> kvp in inputTagThreads)
            {
                kvp.Value.Start();
            }
        }

        public static void StartContextThread()
        {
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    lock (locker)
                    {
                        databaseContext.SaveChanges();
                    }

                    Thread.Sleep(5000);
                }
            });
            t.Start();
        }

        public static bool DriverAddressValidation(string driver, string address)
        {
            if(driver == "SD" && (address != "S" && address != "C" && address != "R"))
                return false;
            
            if (driver == "RTD" && !RealTimeDriver.values.ContainsKey(address))
                return false;
            
            return true;
        }

        internal static string ChangeOutputValue(string tagName, double value)
        {
            Output o = outputTags[tagName];

            if (o.GetType() == typeof(DigitalOutput))
            {
                if (value != 0 && value != 1)
                {
                    return "Pocetna vrednost moze biti samo 0 ili 1.";
                }
            }
            else
            {
                AnalogOutput ao = (AnalogOutput)o;
                if (value > ao.HighLimit || value < ao.LowLimit)
                {
                    return "Pocetna vrednost mora biti izmedju low i high limita.";
                }
            }

            TagValue tagValue = new TagValue { TagName = tagName, Time = DateTime.Now, Value = value, Tag = "output", Type = o.Type };
            using (var db = new DatabaseContext())
            {
                db.Values.Add(tagValue);
                db.SaveChanges();
            }

            outputValues[o.IOAddress] = value;
            return "Uspesna izmena vrednosti.";
        }

        public static void InvokeInputChanged(string input, double value)
        {
            inputChanged?.Invoke(input, value);
        }

        public static void DoWork(Input input)
        {
            //input.OldValue = -100;
            while (true)
            {
                bool onOffScan = inputTags[input.TagName].OnOffScan;
                if (onOffScan)
                {
                    double value = ReadValueFromDriver(input);
                    
                    lock (locker)
                    {
                        
                        if (input.GetType() == typeof(AnalogInput))
                        {
                            AnalogInput ai = (AnalogInput)input;

                            value = value < ai.LowLimit ? ai.LowLimit : value;
                            value = value > ai.HighLimit ? ai.HighLimit : value;

                            
                            CheckAlarm(input, value);
                        } else
                        {
                            value = value < 0.5 ? 0 : 1;
                        }
                        //if(value != input.OldValue)
                        //{
                        //    input.OldValue = value;
                            TagValue tagValue = new TagValue { TagName = input.TagName, Time = DateTime.Now, Value = value, Tag = "input", Type = input.Type };
                            databaseContext.Values.Add(tagValue);
                        //}
                    }

                    InvokeInputChanged(input.TagName, value);
                }

                Thread.Sleep(input.ScanTime * 1000);
            }
        }
        
        public static Thread AddInputTag(Input input)
        {
            inputTags.Add(input.TagName, input);
            Thread t = new Thread(() =>
            {
                DoWork(input);
            });
            inputTagThreads.Add(input.TagName, t);
            return t;
        }
        private static void InvokeAlarm(AlarmValue alarm)
        {
            alarmCalled?.Invoke($"Tag: {alarm.TagName}, Alarm type: {alarm.Type}, Alarm limit: {alarm.Limit}, Trigger Value: {alarm.TriggerValue}, Time {alarm.Time}", alarm.Priority);
        }
        private static void CheckAlarm(Input analogInput, double value)
        {
            using (var db = new DatabaseContext())
            {
                foreach (var alarm in alarms.Values)
                {
                    if (alarm.TagName == analogInput.TagName)
                    {
                        if (alarm.Type == "low" && value <= alarm.Limit)
                        {
                            var alarmValue = new AlarmValue(alarm.Type, alarm.Priority, alarm.TagName, alarm.Limit, DateTime.Now, value);
                            db.Alarms.Add(alarmValue);
                            
                            InvokeAlarm(alarmValue);
                            WriteAlarmsLog(alarmValue);
                        }
                        else if (alarm.Type == "high" && value >= alarm.Limit)
                        {
                            var alarmValue = new AlarmValue(alarm.Type, alarm.Priority, alarm.TagName, alarm.Limit, DateTime.Now, value);
                            db.Alarms.Add(alarmValue);

                            InvokeAlarm(alarmValue);
                            WriteAlarmsLog(alarmValue);
                        }
                    }
                    
                }
                db.SaveChanges();
            }
        }

        private static void WriteAlarmsLog(AlarmValue alarmValue)
        {
            lock (locker)
            {
                using (StreamWriter sw = File.AppendText(HostingEnvironment.ApplicationPhysicalPath + "\\alarmsLog.txt"))
                {
                    using (var db = new DatabaseContext())
                    {
                        sw.WriteLine($"Tag: {alarmValue.TagName}, Alarm type: {alarmValue.Type}, Alarm limit: {alarmValue.Limit}, value: {alarmValue.TriggerValue}, Time {alarmValue.Time}");
                    }
                }
            }
        }

        public static void AddAlarm(Alarm alarm)
        {
            string id = alarm.TagName + alarm.Type + alarm.Limit;
            alarms.Add(id, alarm);
        }

        public static void RemoveAlarm(string id)
        {
            alarms.Remove(id);
        }

        public static void AddOutputTag(Output output)
        {
            outputTags.Add(output.TagName, output);
            if(!outputValues.ContainsKey(output.IOAddress))
            {
                outputValues.Add(output.IOAddress, output.InitialValue);
            }
            
            TagValue tagValue = new TagValue { TagName = output.TagName, Time = DateTime.Now, Value = output.InitialValue, Tag = "output", Type = output.Type };
            using (var db = new DatabaseContext())
            {
                db.Values.Add(tagValue);
                db.SaveChanges();
            }
        }

        public static void RemoveOutputTag(string tagName)
        {
            outputTags.Remove(tagName);
            WriteScadaConfig();
        }

        public static void RemoveInputTag(string tagName)
        {
            inputTags.Remove(tagName);
            inputTagThreads[tagName].Abort();
            inputTagThreads.Remove(tagName);

            List<Alarm> tagAlarms = GetTagAlarms(tagName);
            foreach (var alarm in tagAlarms)
            {
                RemoveAlarm(alarm.TagName + alarm.Type + alarm.Limit);
            }
            
            WriteScadaConfig();
        }

        private static List<Alarm> GetTagAlarms(string tagName)
        {
            List<Alarm> tagAlarms = new List<Alarm>();
            
            foreach(var alarm in alarms.Values)
            {
                if(alarm.TagName == tagName)
                {
                    tagAlarms.Add(alarm);
                }
            }
            return tagAlarms;
        }

        

        public static double ReadValueFromDriver(Input input)
        {
            double value = input.Driver == "SD" ? SimulationDriver.ReturnValue(input.IOAddress) : RealTimeDriver.ReturnValue(input.IOAddress);
            return value;
        }
        private static List<AnalogInput> GetAnalogInputs()
        {
            List<AnalogInput> analogInputList = new List<AnalogInput>();

            foreach (Input i in inputTags.Values)
            {
                if (i.GetType() == typeof(AnalogInput))
                {
                    analogInputList.Add((AnalogInput)i);
                }
            }
            return analogInputList;
        }
        private static List<AnalogOutput> GetAnalogOutputs()
        {
            List<AnalogOutput> analogOutputList = new List<AnalogOutput>();

            foreach (Output o in outputTags.Values)
            {
                if (o.GetType() == typeof(AnalogOutput))
                {
                    analogOutputList.Add((AnalogOutput)o);
                }
            }
            return analogOutputList;
        }
        public static void WriteScadaConfig()
        {
            XElement scadaXML = new XElement("Tags",
                from input in inputTags
                where input.Value.Type == "digital"
                select new XElement("Tag", input.Value.Description, new XAttribute("tag", "input"),
                                                                    new XAttribute("type", input.Value.Type),
                                                                    new XAttribute("name", input.Value.TagName),
                                                                    new XAttribute("address", input.Value.IOAddress),
                                                                    new XAttribute("driver", input.Value.Driver),
                                                                    new XAttribute("scanTime", input.Value.ScanTime),
                                                                    new XAttribute("onOffScan", input.Value.OnOffScan)),
                from analogInput in GetAnalogInputs()
                select new XElement("Tag", analogInput.Description, new XAttribute("tag", "input"),
                                                                    new XAttribute("type", analogInput.Type),
                                                                    new XAttribute("name", analogInput.TagName),
                                                                    new XAttribute("address", analogInput.IOAddress),
                                                                    new XAttribute("driver", analogInput.Driver),
                                                                    new XAttribute("scanTime", analogInput.ScanTime),
                                                                    new XAttribute("onOffScan", analogInput.OnOffScan),
                                                                    new XAttribute("lowLimit", analogInput.LowLimit),
                                                                    new XAttribute("highLimit", analogInput.HighLimit)),
                from output in outputTags
                where output.Value.Type == "digital"
                select new XElement("Tag", output.Value.Description, new XAttribute("tag", "output"),
                                                                     new XAttribute("type", output.Value.Type),
                                                                     new XAttribute("name", output.Value.TagName),
                                                                     new XAttribute("address", output.Value.IOAddress),
                                                                     new XAttribute("initValue", output.Value.InitialValue)),
                from analogOutput in GetAnalogOutputs()
                select new XElement("Tag", analogOutput.Description, new XAttribute("tag", "output"),
                                                                new XAttribute("type", analogOutput.Type),
                                                                new XAttribute("name", analogOutput.TagName),
                                                                new XAttribute("address", analogOutput.IOAddress),
                                                                new XAttribute("initValue", analogOutput.InitialValue),
                                                                new XAttribute("lowLimit", analogOutput.LowLimit),
                                                                new XAttribute("highLimit", analogOutput.HighLimit)
                ));
            using (StreamWriter sw = File.CreateText(HostingEnvironment.ApplicationPhysicalPath + "\\scadaConfig.xml"))
            {
                sw.Write(scadaXML);
            }
        }
        
        public static void WriteAlarmConfig()
        {
            List<AlarmValue> alarmList = new List<AlarmValue>();
            
            XElement alarmXML = new XElement("Alarms",
                from alarm in alarms.Values
                select new XElement("Alarm", alarm.Limit, new XAttribute("type", alarm.Type),
                                                            new XAttribute("tagName", alarm.TagName),
                                                            new XAttribute("priority", alarm.Priority)
                                                            ));
            
            using (StreamWriter sw = File.CreateText(HostingEnvironment.ApplicationPhysicalPath + "\\alarmConfig.xml"))
            {
                sw.Write(alarmXML);
            }
        }
        
    }
}