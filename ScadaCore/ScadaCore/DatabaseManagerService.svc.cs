using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Web.Hosting;

namespace ScadaCore
{
    public class DatabaseManagerService : IDatabaseManagerService
    {
        

        public string AddInputTag(string tagName, string desc, string address, string driver, int scanTime, bool onOffScan, double lowLimit, double highLimit, string type)
        {
            if (TagProcessing.inputTags.ContainsKey(tagName))
            {
                return "Tag sa tim id-jem vec postoji.";
            }
            if(!TagProcessing.DriverAddressValidation(driver, address))
            {
                return "Unos drivera nije dobar.";
            }
            if (type == "digital")
            {
                DigitalInput digitalInput = new DigitalInput(tagName, desc, driver, address, scanTime, onOffScan, "digital");
                Thread t = TagProcessing.AddInputTag(digitalInput);
                t.Start();
            } else
            {
                AnalogInput analogInput = new AnalogInput(lowLimit, highLimit, tagName, desc, driver, address, scanTime, onOffScan, "analog");
                Thread t = TagProcessing.AddInputTag(analogInput);
                t.Start();
            }
            TagProcessing.WriteScadaConfig();
            return "Tag uspesno dodat.";
        }

        public string AddOutputTag(string tagName, string desc, string address, double initVal, double lowLimit, double highLimit, string type)
        {
            if (TagProcessing.outputTags.ContainsKey(tagName))
            {
                return "Tag sa tim id-jem vec postoji.";
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
            return "Tag uspesno dodat.";
        }

        public string ChangeOutputValue(string tagName, double value)
        {
            if(!TagProcessing.outputTags.ContainsKey(tagName))
            {
                return "Ne postoji tag sa tim id-jem";
            }

            return TagProcessing.ChangeOutputValue(tagName, value);
        }

        public Dictionary<string, double> GetOutputValues()
        {
            return TagProcessing.outputValues;
        }

        public string TurnScanOnOff(string tagName)
        {
            if(!TagProcessing.inputTags.ContainsKey(tagName))
            {
                return "Ne postoji tag sa tim id-jem";
            }
            bool scan = TagProcessing.inputTags[tagName].OnOffScan;
            TagProcessing.inputTags[tagName].OnOffScan = !scan;
            TagProcessing.WriteScadaConfig();

            if (scan)
                return "Tag uspesno iskljucen.";
            else
                return "Tag uspesno ukljucen.";
        }

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

        public string RemoveInputTag(string tagName)
        {
            if (!TagProcessing.inputTags.ContainsKey(tagName))
            {
                return "Tag sa tim id-jem ne postoji.";
            }
            TagProcessing.RemoveInputTag(tagName);
            return "Tag uspesno obrisan.";
        }

        public string RemoveOutputTag(string tagName)
        {
            if (!TagProcessing.outputTags.ContainsKey(tagName))
            {
                return "Tag sa tim id-jem ne postoji.";
            }
            TagProcessing.RemoveOutputTag(tagName);
            return "Tag uspesno obrisan.";
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
                var user = db.Users.Where(u => u.Username == username).FirstOrDefault();
                if (user != null)
                {
                    if (ValidateEncryptedData(password, user.Password))
                    {
                        return user.Role;
                    }
                    return "Lozinka nije dobra.";
                }
                return "Korisnicko ime ne postoji.";
            }
        }

        public string AddAlarm(string tagName, string type, double limit, int priority)
        {
            if (TagProcessing.inputTags.ContainsKey(tagName) && !TagProcessing.alarms.ContainsKey(tagName + type + limit))
            {
                if (TagProcessing.inputTags[tagName].GetType() == typeof(AnalogInput))
                {
                    AnalogInput ai = (AnalogInput)TagProcessing.inputTags[tagName];
                    if (type == "low" && limit < ai.LowLimit)
                    {
                        return "Neuspesno dodavanje alarma. Limit alarma je manji od limita inputa";
                    }
                    else if (type == "high" && limit > ai.HighLimit)
                    {
                        return "Neuspesno dodavanje alarma. Limit alarma je veci od limita inputa";
                    }
                    TagProcessing.AddAlarm(new Alarm(type, priority, tagName, limit));
                    TagProcessing.WriteAlarmConfig();
                    return "Alarm uspesno dodat.";
                }
                return "Tag sa tim id-jem nije analogni.";
            }
            return "Ne postoji tag sa tim id-jem ili alarm vec postoji.";
        }

        public string DeleteAlarm(string id)
        {
            if (TagProcessing.alarms.ContainsKey(id))
            {
                TagProcessing.RemoveAlarm(id);
                TagProcessing.WriteAlarmConfig();
                return "Alarm uspesno obrisan.";
            }
            return "Ne postoji alarm sa tim id-jem.";
        }
        private static bool ValidateEncryptedData(string valueToValidate, string valueFromDatabase)
        {
            string[] arrValues = valueFromDatabase.Split(':');
            string encryptedDbValue = arrValues[0];
            string salt = arrValues[1];
            byte[] saltedValue = Encoding.UTF8.GetBytes(salt + valueToValidate);
            using (var sha = new SHA256Managed())
            {
                byte[] hash = sha.ComputeHash(saltedValue);
                string enteredValueToValidate = Convert.ToBase64String(hash);
                return encryptedDbValue.Equals(enteredValueToValidate);
            }
        }
    }
}
