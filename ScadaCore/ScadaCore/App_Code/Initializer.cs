using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

namespace ScadaCore.App_Code
{
    public class Initializer
    {
        public static void AppInitialize()
        {
            XElement scadaXML = XElement.Load(HostingEnvironment.ApplicationPhysicalPath + "\\scadaConfig.xml");

            var tags = scadaXML.Descendants("Tag").Where(t => t.Attribute("driver").Value == "SD");
            InitializeTags(tags);
            TagProcessing.AddInputTagsToDatabase();
            InitializeAlarms();
            TagProcessing.StartContextThread();
            TagProcessing.StartThreads();
        }
        private static void InitializeTags(IEnumerable<XElement> tags)
        {
            foreach(var tag in tags)
            {
                string type = tag.Attribute("type").Value;
                string name = tag.Attribute("name").Value;
                string address = tag.Attribute("address").Value;
                string tagType = tag.Attribute("tag").Value;
                string description = tag.Value;
                string driver;
                int scanTime, lowLimit, highLimit;
                bool onOffScan;
                double initValue;

                if(tagType == "input")
                {
                    driver = tag.Attribute("driver").Value;
                    scanTime = Convert.ToInt32(tag.Attribute("scanTime").Value);
                    onOffScan = Convert.ToBoolean(tag.Attribute("onOffScan").Value);

                    if(type == "analog")
                    {
                        lowLimit = Convert.ToInt32(tag.Attribute("lowLimit").Value);
                        highLimit = Convert.ToInt32(tag.Attribute("highLimit").Value);

                        AnalogInput analogInput = new AnalogInput(lowLimit, highLimit, name, description, driver, address, scanTime, onOffScan, "analog");
                        TagProcessing.AddInputTag(analogInput);
                    } else
                    {
                        DigitalInput digitalInput = new DigitalInput(name, description, driver, address, scanTime, onOffScan, "digital");
                        TagProcessing.AddInputTag(digitalInput);
                    }
                } else
                {
                    initValue = Convert.ToDouble(tag.Attribute("initValue").Value);
                    if (type == "analog")
                    {
                        lowLimit = Convert.ToInt32(tag.Attribute("lowLimit").Value);
                        highLimit = Convert.ToInt32(tag.Attribute("highLimit").Value);
                        AnalogOutput analogOutput = new AnalogOutput(lowLimit, highLimit, name, description, address, initValue, "analog");
                        TagProcessing.AddOutputTag(analogOutput);
                    } else
                    {
                        DigitalOutput digitalOutput = new DigitalOutput(name, description, address, initValue, "digital");
                        TagProcessing.AddOutputTag(digitalOutput);
                    }
                    
                    
                }
            }
        }

        private static void InitializeAlarms()
        {
            XElement alarmXML = XElement.Load(HostingEnvironment.ApplicationPhysicalPath + "\\alarmConfig.xml");
            var alarms = alarmXML.Descendants("Alarm");
            foreach (var alarm in alarms)
            {
                string tagName = alarm.Attribute("tagName").Value;
                string type = alarm.Attribute("type").Value;
                int priority = Convert.ToInt32(alarm.Attribute("priority").Value);
                DateTime time = DateTime.Parse(alarm.Attribute("time").Value);
                double limit = Double.Parse(alarm.Value);

                string id = tagName + type + limit;
                TagProcessing.AddAlarm(new Alarm(id, type, priority, time, tagName, limit));
            }
        }

    }
}