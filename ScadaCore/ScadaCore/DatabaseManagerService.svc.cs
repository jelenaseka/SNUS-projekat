using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Web.Hosting;

namespace ScadaCore
{
    public class DatabaseManagerService : IDatabaseManagerService
    {
        //public void addAlarm(string tagName, string type, double limit, int priority, DateTime time)
        //{
        //    //if(TagProcessing.inputTags.ContainsKey(tagName))
        //    //{
        //    //    if(TagProcessing.inputTags[tagName].GetType() == typeof(AnalogInput))
        //    //    {
        //    //        TagProcessing.alarms.Add(new Alarm(type, priority, time, tagName, limit));
        //    //        TagProcessing.writeAlarmConfig();
        //    //    }
        //    //}
            
        //}

        public bool AddInputTag(string tagName, string desc, string address, string driver, int scanTime, bool onOffScan, int lowLimit, int highLimit, string type)
        {
            if (TagProcessing.inputTags.ContainsKey(tagName))
            {
                return false;
            }
            if(!TagProcessing.DriverAddressValidation(driver, address))
            {
                return false;
            }
            if (type == "digital")
            {
                DigitalInput digitalInput = new DigitalInput(tagName, desc, driver, address, scanTime, onOffScan, "digital");
                Thread t = TagProcessing.AddInputTag(digitalInput);
                TagProcessing.AddInputTagToDatabase(digitalInput);
                t.Start();
            } else
            {
                AnalogInput analogInput = new AnalogInput(lowLimit, highLimit, tagName, desc, driver, address, scanTime, onOffScan, "analog");
                Thread t = TagProcessing.AddInputTag(analogInput);
                TagProcessing.AddInputTagToDatabase(analogInput);
                t.Start();
            }
            TagProcessing.WriteScadaConfig();
            return true;
        }

        public bool AddOutputTag(string tagName, string desc, string address, double initVal, int lowLimit, int highLimit, string type)
        {
            if (TagProcessing.outputTags.ContainsKey(tagName))
            {
                return false;
            }
            if (type == "digital")
            {
                DigitalOutput digitalOutput = new DigitalOutput(tagName, desc, address, initVal, "digital");
                TagProcessing.AddOutputTag(digitalOutput);
            }
            else
            {
                AnalogOutput analogOutput = new AnalogOutput(lowLimit, highLimit, tagName, desc, address, initVal, "analog");
                TagProcessing.AddOutputTag(analogOutput);
            }
            TagProcessing.WriteScadaConfig();
            return true;
        }

        public bool ChangeOutputValue(string tagName, double value)
        {
            lock(TagProcessing.locker)
            {
                var output = TagProcessing.databaseContext.Values
                        .Where(tag => tag.TagName == tagName)
                        .FirstOrDefault();
                if (output == null)
                {
                    return false;
                }
                output.Value = value;
                TagProcessing.databaseContext.SaveChanges();
            }
            
            return true;
        }

        public Dictionary<string, double> GetOutputValues()
        {
            Dictionary<string, double> values = new Dictionary<string, double>();

            lock(TagProcessing.locker)
            {
                var outputs = TagProcessing.databaseContext.Values
                            .Where(tag => tag.Tag == "output");
                foreach (var tag in outputs) 
                {
                    values.Add(tag.TagName, tag.Value);
                }
            }

            return values;
        }

        public bool TurnScanOnOff(string tagName, bool scan)
        {
            if(!TagProcessing.inputTags.ContainsKey(tagName))
            {
                return false;
            }
            TagProcessing.inputTags[tagName].OnOffScan = scan;
            TagProcessing.WriteScadaConfig();
            
            return true;
        }

        //public void deleteAlarm(string tagName)
        //{
        //    //List<Alarm> newList = new List<Alarm>();
        //    //foreach(var alarm in TagProcessing.alarms)
        //    //{
        //    //    if (alarm.TagName != tagName)
        //    //    {
        //    //        newList.Add(alarm);
        //    //    }
        //    //}
        //    //TagProcessing.alarms = newList;
        //    //TagProcessing.writeAlarmConfig(newList);
        //}


        public bool IsDatabaseEmpty()
        {
            using (var db = new DatabaseContext())
            {
                if (!db.Users.Any())
                {
                    return true;
                }
            }
            return false;
        }

        public bool RemoveInputTag(string tagName)
        {
            return TagProcessing.RemoveInputTag(tagName);
        }

        public bool RemoveOutputTag(string tagName)
        {
            return TagProcessing.RemoveOutputTag(tagName);
        }

        public bool Registration(string username, string password, string role)
        {
            using (var db = new DatabaseContext())
            {

                var userExists = db.Users.Where(user => user.Username == username).FirstOrDefault();

                if (userExists != null)
                {
                    return false;
                }

                User userToAdd = new User(username, password, role);
                db.Users.Add(userToAdd);
                db.SaveChanges();
            }
            return true;
        }

        public string SignIn(string username, string password)
        {
            using (var db = new DatabaseContext())
            {
                var user = db.Users.Where(u => u.Username == username && u.Password == password).FirstOrDefault();
                if (user != null)
                {
                    return user.Role;
                }
                return null;
            }
        }

    }
}
