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
        public delegate void AlarmCalledDelegate(string alarm);
        public static event AlarmCalledDelegate alarmCalled;
        public static Dictionary<string, Thread> inputTagThreads = new Dictionary<string, Thread>();
        public static Dictionary<string, Input> inputTags = new Dictionary<string, Input>();
        public static Dictionary<string, Output> outputTags = new Dictionary<string, Output>();
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
                    lock(locker)
                    {
                        databaseContext.SaveChanges();
                        //using (StreamWriter sw = File.AppendText(HostingEnvironment.ApplicationPhysicalPath + "\\bzvz.txt"))
                        //{
                        //    foreach (var val in databaseContext.Values)
                        //    {
                        //        sw.WriteLine($"Tag: {val.Tag}, Type: {val.Type}, Tag name: {val.TagName}, Time: {val.Time}, Value: {val.Value}");
                        //    }
                        //    sw.WriteLine("----------------------------------------------------");
                        //    foreach(var alarm in databaseContext.Alarms)
                        //    {
                        //        sw.WriteLine(alarm.Id);
                        //    }
                        //}
                    }

                    Thread.Sleep(10000);
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

        public static void InvokeInputChanged(string input, double value)
        {
            inputChanged?.Invoke(input, value);
        }

        public static void DoWork(Input input)
        {
            while (true)
            {
                bool onOffScan = inputTags[input.TagName].OnOffScan;
                if (onOffScan)
                {
                    double value = ReadValueFromDriver(input);
                    lock (locker)
                    {

                        var checkTag = databaseContext.Values
                                    .Where(tag => tag.TagName == input.TagName)
                                    .FirstOrDefault();
                        
                        checkTag.Value = value;

                        if (input.GetType() == typeof(AnalogInput))
                        {
                            CheckAlarm(input, value);
                        }
                    }

                    InvokeInputChanged(input.TagName, value);
                }

                Thread.Sleep(input.ScanTime * 1000);
            }
        }
        public static void AddInputTagToDatabase(Input tag)
        {
            lock (locker)
            {
                var checkTag = databaseContext.Values
                                    .Where(tag1 => tag1.TagName == tag.TagName)
                                    .FirstOrDefault();
                TagValue tagValue = new TagValue { TagName = tag.TagName, Time = DateTime.Now, Value = 0, Tag = "input", Type = tag.Type };
                if (checkTag == null)
                {
                    databaseContext.Values.Add(tagValue);
                }
                databaseContext.SaveChanges();
            }
        }
        public static void AddInputTagsToDatabase()
        {
            foreach(var tag in inputTags.Values)
            {
                AddInputTagToDatabase(tag);
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
        private static void InvokeAlarm(Alarm alarm, double value)
        {
            alarmCalled?.Invoke($"Tag: {alarm.TagName}, Alarm type: {alarm.Type}, Alarm limit: {alarm.Limit}, value: {value}, Time {alarm.Time}");
        }
        private static void CheckAlarm(Input analogInput, double value)
        {
            foreach(var alarm in databaseContext.Alarms)
            {
                if(alarm.TagName == analogInput.TagName)
                {
                    if(alarm.Type == "low" && value <= alarm.Limit)
                    {
                        InvokeAlarm(alarm, value);
                    } else if(alarm.Type == "high" && value >= alarm.Limit)
                    {
                        InvokeAlarm(alarm, value);
                    }
                }
                using (StreamWriter sw = File.AppendText(HostingEnvironment.ApplicationPhysicalPath + "\\alarmsLog.txt"))
                {
                    sw.WriteLine($"Tag: {alarm.TagName}, Alarm type: {alarm.Type}, Alarm limit: {alarm.Limit}, value: {value}, Time {alarm.Time}");
                }
                
            }

        }
        public static void AddAlarm(Alarm alarm)
        {
            var checkAlarm = databaseContext.Alarms
                                    .Where(alarm1 => alarm1.Id == (alarm.TagName + alarm.Type + alarm.Limit))
                                    .FirstOrDefault();
            if(checkAlarm == null)
            {
                databaseContext.Alarms.Add(alarm);
                databaseContext.SaveChanges();
            }
            
        }

        
        public static void AddOutputTag(Output output)
        {
            outputTags.Add(output.TagName, output);
            TagValue tagValue = new TagValue { TagName = output.TagName, Time = DateTime.Now, Value = output.InitialValue, Tag = "output", Type = output.Type };
            lock (locker)
            {
                var checkTag = databaseContext.Values
                                    .Where(tag => tag.TagName == output.TagName)
                                    .FirstOrDefault();
                
                if (checkTag == null)
                {
                    databaseContext.Values.Add(tagValue);
                    databaseContext.SaveChanges();
                }
            }
            
            
        }

        internal static bool RemoveOutputTag(string tagName)
        {
            if (!outputTags.ContainsKey(tagName))
            {
                return false;
            }
            outputTags.Remove(tagName);
            lock (locker)
            {
                var tag = databaseContext.Values
                            .Where(t => t.TagName == tagName)
                            .FirstOrDefault();
                databaseContext.Values.Remove(tag);
                databaseContext.SaveChanges();
            }
            WriteScadaConfig();
            return true;
        }

        public static bool RemoveInputTag(string tagName)
        {
            if(!inputTags.ContainsKey(tagName))
            {
                return false;
            }
            inputTags.Remove(tagName);
            inputTagThreads[tagName].Abort();
            inputTagThreads.Remove(tagName);

            lock (locker)
            {
                var tag = databaseContext.Values
                            .Where(t => t.TagName == tagName)
                            .FirstOrDefault();
                databaseContext.Values.Remove(tag);
                databaseContext.SaveChanges();
            }

            List<Alarm> tagAlarms = GetTagAlarms(tagName);
            foreach (var alarm in tagAlarms)
            {
                RemoveAlarm(alarm.Id);
            }
            
            WriteScadaConfig();
            return true;
        }

        private static List<Alarm> GetTagAlarms(string tagName)
        {
            List<Alarm> tagAlarms = new List<Alarm>();
            lock(locker)
            {
                var alarms = databaseContext.Alarms
                        .Where(a => a.TagName == tagName);
                foreach(var alarm in alarms)
                {
                    tagAlarms.Add(alarm);
                }
            }
            return tagAlarms;
        }

        private static void RemoveAlarm(string id)
        {
            lock(locker)
            {
                var alarm = databaseContext.Alarms
                        .Where(a => a.Id == id)
                        .FirstOrDefault();
                databaseContext.Alarms.Remove(alarm);
                databaseContext.SaveChanges();
            }
            
            WriteAlarmConfig();
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
        //public static void UpdateScadaConfig(string tagName, string attribute, string value)
        //{
        //    XElement oldScadaXML = XElement.Load(HostingEnvironment.ApplicationPhysicalPath + "\\scadaConfig.xml");
        //    var oldTags = oldScadaXML.Descendants("Tag");
        //    //oldScadaXML.Add(new XElement("Tag"));
        //    foreach (var elem in oldTags)
        //    {
        //        if (elem.Attribute("name").Value == tagName)
        //        {
        //            elem.Attribute(attribute).Value = value;
        //        }
        //    }
        //    XElement scadaXML = new XElement("Tags", oldTags);
        //    using (StreamWriter sw = File.CreateText(HostingEnvironment.ApplicationPhysicalPath + "\\scadaConfig.xml"))
        //    {
        //        sw.Write(scadaXML);
        //    }
        //}
        public static void WriteAlarmConfig()
        {
            List<Alarm> alarmList = new List<Alarm>();
            lock(locker)
            {
                var alarms = databaseContext.Alarms;
                foreach (var alarm in alarms)
                {
                    alarmList.Add(alarm);
                }
            }
            
            XElement alarmXML = new XElement("Alarms",
                from alarm in databaseContext.Alarms.ToArray()
                select new XElement("Alarm", alarm.Limit, new XAttribute("type", alarm.Type),
                                                          new XAttribute("tagName", alarm.TagName),
                                                          new XAttribute("priority", alarm.Priority),
                                                          new XAttribute("time", alarm.Time.ToString())));
            using (StreamWriter sw = File.CreateText(HostingEnvironment.ApplicationPhysicalPath + "\\alarmConfig.xml"))
            {
                sw.Write(alarmXML);
            }
        }
        
        //public static void writeScadaConfig()
        //{
        //    List<AnalogInput> analogInputList = new List<AnalogInput>();
        //    List<AnalogOutput> analogOutputList = new List<AnalogOutput>();

        //    foreach (DigitalInput di in inputTags.Values)
        //    {
        //        if (di.GetType() == typeof(AnalogInput))
        //        {
        //            analogInputList.Add((AnalogInput)di);
        //        }
        //    }

        //    foreach (DigitalOutput doo in outputTags.Values)
        //    {
        //        if (doo.GetType() == typeof(AnalogOutput))
        //        {
        //            analogOutputList.Add((AnalogOutput)doo);
        //        }
        //    }

        //    XElement scadaXML = new XElement("Tags",
        //        from input in inputTags
        //        where input.Value.Tag == "digital"
        //        select new XElement("Tag", input.Value.Description, new XAttribute("type", "input"),
        //                                                            new XAttribute("tag", input.Value.Tag),
        //                                                            new XAttribute("name", input.Value.TagName),
        //                                                            new XAttribute("address", input.Value.IOAddress),
        //                                                            new XAttribute("driver", input.Value.Driver),
        //                                                            new XAttribute("scanTime", input.Value.ScanTime),
        //                                                            new XAttribute("onOffScan", input.Value.OnOffScan)),
        //        from input2 in analogInputList
        //        select new XElement("Tag", input2.Description, new XAttribute("type", "input"),
        //                                                            new XAttribute("tag", input2.Tag),
        //                                                            new XAttribute("name", input2.TagName),
        //                                                            new XAttribute("address", input2.IOAddress),
        //                                                            new XAttribute("driver", input2.Driver),
        //                                                            new XAttribute("scanTime", input2.ScanTime),
        //                                                            new XAttribute("onOffScan", input2.OnOffScan),
        //                                                            new XAttribute("lowLimit", input2.LowLimit),
        //                                                            new XAttribute("highLimit", input2.HighLimit)),
        //        from output in outputTags
        //        where output.Value.Tag == "digital"
        //        select new XElement("Tag", output.Value.Description, new XAttribute("type", "output"),
        //                                                             new XAttribute("tag", output.Value.Tag),
        //                                                             new XAttribute("name", output.Value.TagName),
        //                                                             new XAttribute("address", output.Value.IOAddress),
        //                                                             new XAttribute("initValue", output.Value.InitialValue)),
        //        from output2 in analogOutputList
        //        select new XElement("Tag", output2.Description, new XAttribute("type", "output"),
        //                                                        new XAttribute("tag", output2.Tag),
        //                                                        new XAttribute("name", output2.TagName),
        //                                                        new XAttribute("address", output2.IOAddress),
        //                                                        new XAttribute("initValue", output2.InitialValue),
        //                                                        new XAttribute("lowLimit", output2.LowLimit),
        //                                                        new XAttribute("highLimit", output2.HighLimit)
        //        ));
        //    using (StreamWriter sw = File.CreateText(HostingEnvironment.ApplicationPhysicalPath + "\\scadaConfig.xml"))
        //    {
        //        sw.Write(scadaXML);
        //    }
        //}
    }
}