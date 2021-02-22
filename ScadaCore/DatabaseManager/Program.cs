using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DatabaseManager
{
    class Program
    {
        static ServiceReference.DatabaseManagerServiceClient dbClient = new ServiceReference.DatabaseManagerServiceClient();
        private static bool signedIn = false;
        private static string role;
        static void Main(string[] args)
        {
            Console.WriteLine("pocetak");
            while (true)
            {
                if (dbClient.IsDatabaseEmpty())
                    RegisterMenu();
                else if (signedIn)
                    OptionsMenu();
                else
                    SignInMenu();
            }

        }
        private static void RegisterMenu()
        {
            PrintRegisterMenu();
            int option = ChoseOption();
            ExecuteRegisterOption(option);
        }
        private static void SignInMenu()
        {
            PrintSignInMenu();
            int option = ChoseOption();
            if (option == 1) SignIn();
            else if (option == 0) System.Environment.Exit(1);
            else Console.WriteLine("Pogresan unos.");
        }
        private static void OptionsMenu()
        {
            PrintOptionsMenu();
            int option = ChoseOption();
            ExecuteOptionsMenu(option);
        }
        private static int ChoseOption()
        {
            bool success = false;
            int chosen = -1;
            while (!success)
            {
                success = Int32.TryParse(Console.ReadLine(), out chosen);
                if(!success)
                {
                    Console.WriteLine("Pogresan unos. Pokusajte ponovo");
                }
            }
            return chosen;
        }
        private static void ExecuteRegisterOption(int option)
        {
            switch (option)
            {
                case 0: System.Environment.Exit(1); break;
                case 1: Registration(); break;
                default: Console.WriteLine("Pogresan unos"); break;
            }
        }

        private static void Registration()
        {
            Console.WriteLine("--------------Registracija--------------");
            Console.WriteLine("Korisnicko ime:");
            string username = Console.ReadLine();
            Console.WriteLine("Lozinka:");
            string password = Console.ReadLine();

            if (IsEmpty(username, password))
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }

            string role = dbClient.IsDatabaseEmpty() ? "admin" : "moderator";

            if (dbClient.Registration(username, password, role))
                Console.WriteLine("Registracija uspesno obavljena.");
            else
                Console.WriteLine("Registracija neuspesna.");
        }

        private static void SignIn()
        {
            Console.WriteLine("Korisnicko ime:");
            string username = Console.ReadLine();
            Console.WriteLine("Lozinka:");
            string password = Console.ReadLine();
            if (IsEmpty(username, password))
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }
            role = dbClient.SignIn(username, password);
            if (role != null)
                signedIn = true;
            else Console.WriteLine("Neuspesna prijava.");
        }
        private static void SignOut()
        {
            signedIn = false;
            Console.WriteLine("Korisnik odjavljen.");
        }
        private static void ExecuteOptionsMenu(int option)
        {
            switch (option)
            {
                case 0: System.Environment.Exit(1); break;
                case 1: ChangeOutputValue(); break;
                case 2: GetOutputValues(); break;
                case 3: TurnScanOnOff(); break;
                case 4: AddInputTag(); break;
                case 5: AddOutputTag(); break;
                case 7: RemoveInputTag(); break;
                case 8: RemoveOutputTag(); break;
                case 9: SignOut(); break;
                case 10:
                    if(role == "admin")
                    {
                        Registration();
                    } else
                    {
                        Console.WriteLine("Niste ovlasceni za ovu akciju.");
                    }
                    break;
                default: Console.WriteLine("Pogresan unos"); break;
            }
        }

        private static void RemoveOutputTag()
        {
            Console.WriteLine("Unesite id taga:");
            string tagName = Console.ReadLine();
            if (dbClient.RemoveOutputTag(tagName))
            {
                Console.WriteLine("Tag uspesno obrisan.");
            }
            else
            {
                Console.WriteLine("Neuspesno brisanje taga.");
            }
        }

        private static void RemoveInputTag()
        {
            Console.WriteLine("Unesite id taga:");
            string tagName = Console.ReadLine();
            if(dbClient.RemoveInputTag(tagName))
            {
                Console.WriteLine("Tag uspesno obrisan.");
            }
            else
            {
                Console.WriteLine("Neuspesno brisanje taga.");
            }
        }

        private static void AddOutputTag()
        {
            Console.WriteLine("Izbor:\n1. Analogni\n2. Digitalni");
            string choice = Console.ReadLine();
            if (choice != "1" && choice != "2")
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }
            Console.WriteLine("Naziv taga:");
            string tagName = Console.ReadLine();
            Console.WriteLine("Opis:");
            string description = Console.ReadLine();
            Console.WriteLine("I/O Adresa:");
            string address = Console.ReadLine();
            Console.WriteLine("Pocetna vrednost:");
            double initValue;
            bool success = Double.TryParse(Console.ReadLine(), out initValue);
            if (IsEmpty(tagName, description, address) || !success)
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }
            if (choice == "1")
            {
                AddAnalogOutput(tagName, description, address, initValue);
            }
            else
            {
                AddDigitalOutput(tagName, description, address, initValue);
            }
        }

        private static void AddAnalogOutput(string tagName, string description, string address, double initValue)
        {
            Console.WriteLine("Low limit:");
            int lowLimit;

            int highLimit;
            bool lowLimitSuccess = Int32.TryParse(Console.ReadLine(), out lowLimit);
            Console.WriteLine("High limit:");
            bool highLimitSuccess = Int32.TryParse(Console.ReadLine(), out highLimit);
            if (!(lowLimitSuccess && highLimitSuccess))
            {
                Console.WriteLine("Pogresan unos");
                return;
            }
            if(dbClient.AddOutputTag(tagName, description, address, initValue, lowLimit, highLimit, "analog"))
            {
                Console.WriteLine("Tag uspesno dodat.");
            } else
            {
                Console.WriteLine("Neuspesno dodavanje taga.");
            }
        }

        private static void AddDigitalOutput(string tagName, string description, string address, double initValue)
        {
            if(dbClient.AddOutputTag(tagName, description, address, initValue, 0, 0, "digital"))
            {
                Console.WriteLine("Tag uspesno dodat.");
            }
            else
            {
                Console.WriteLine("Neuspesno dodavanje taga.");
            }
        }

        private static void AddInputTag()
        {
            Console.WriteLine("Izbor:\n1. Analogni\n2. Digitalni");
            string choice, tagName, description, address, driver;

            choice = Console.ReadLine();
            if (choice != "1" && choice != "2")
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }
            Console.WriteLine("Naziv taga:");
            tagName = Console.ReadLine();
            Console.WriteLine("Opis:");
            description = Console.ReadLine();
            Console.WriteLine("I/O Adresa:");
            address = Console.ReadLine();
            Console.WriteLine("Driver(SD/RTD):");
            driver = Console.ReadLine();
            Console.WriteLine("Scan time:");
            int scanTime;
            bool scanTimeSuccess = Int32.TryParse(Console.ReadLine(), out scanTime);
            Console.WriteLine("On/Off scan:");
            bool onOffScan;
            bool onOffSuccess = Boolean.TryParse(Console.ReadLine(), out onOffScan);
            if(!ValidateInput(tagName, description, address, driver, scanTimeSuccess, onOffSuccess))
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }
           
            if (choice == "1")
            {
                AddAnalogInput(tagName, description, address, driver, scanTime, onOffScan);
            }
            else
            {
                AddDigitalInput(tagName, description, address, driver, scanTime, onOffScan);
            }
        }

        private static bool ValidateInput(string tagName, string description, string address, string driver, bool scanTimeSuccess, bool onOffSuccess)
        {
            return (IsEmpty(tagName, description, address) || !(scanTimeSuccess && onOffSuccess) || (driver != "SD" && driver != "RTD")) ? false : true;
        }

        private static void AddDigitalInput(string tagName, string description, string address, string driver, int scanTime, bool onOffScan)
        {
            if(dbClient.AddInputTag(tagName, description, address, driver, scanTime, onOffScan, 0, 0, "digital"))
            {
                Console.WriteLine("Uspesno dodavanje taga.");
            } else
            {
                Console.WriteLine("Tag sa tim id-jem vec postoji.");
            }
        }

        private static void AddAnalogInput(string tagName, string description, string address, string driver, int scanTime, bool onOffScan)
        {
            int lowLimit;
            int highLimit;
            Console.WriteLine("Low limit:");
            bool lowLimitSuccess = Int32.TryParse(Console.ReadLine(), out lowLimit);
            Console.WriteLine("High limit:");
            bool highLimitSuccess = Int32.TryParse(Console.ReadLine(), out highLimit);
            if (!(lowLimitSuccess && highLimitSuccess))
            {
                Console.WriteLine("Pogresan unos");
                return;
            }
            if(dbClient.AddInputTag(tagName, description, address, driver, scanTime, onOffScan, lowLimit, highLimit, "analog"))
            {
                Console.WriteLine("Uspesno dodavanje taga.");
            }
            else
            {
                Console.WriteLine("Tag sa tim id-jem vec postoji.");
            }
        }

        private static void TurnScanOnOff()
        {
            Console.WriteLine("Unesite id ulaznog taga:");
            string tagName = Console.ReadLine();
            Console.WriteLine("Postavite vrednost:");
            bool onOff;
            bool success = Boolean.TryParse(Console.ReadLine(), out onOff);
            if (success)
            {
                if (dbClient.TurnScanOnOff(tagName, onOff))
                {
                    Console.WriteLine("Uspesno izmenjen on/off scan taga");
                }
                else
                {
                    Console.WriteLine("Neuspesno.");
                }
            } else
            {
                Console.WriteLine("Pogresan unos.");
            }
        }
        private static void GetOutputValues()
        {
            Dictionary<string, double> values = dbClient.GetOutputValues();
            
            foreach (KeyValuePair<string, double> kvp in values)
            {
                Console.WriteLine($"TagName = {kvp.Key}, Value = {kvp.Value}");
            }
        }
        private static void ChangeOutputValue()
        {
            Console.WriteLine("Unesite id taga:");
            string tagName = Console.ReadLine();
            Console.WriteLine("Unesite novu vrednost:");
            double newValue;
            if (Double.TryParse(Console.ReadLine(), out newValue))
            {
                if (dbClient.ChangeOutputValue(tagName, newValue))
                {
                    Console.WriteLine("Uspesna izmena vrednosti.");
                }
                else
                {
                    Console.WriteLine("Neuspesna izmena vrednosti. Id taga ne postoji.");
                }
            }
            else
            {
                Console.WriteLine("Pogresan unos.");
            }
        }
        private static void PrintRegisterMenu()
        {
            Console.WriteLine("Registrujte prvog korisnika\n");
            Console.WriteLine("1. Registracija");
            Console.WriteLine("0 <-- EXIT");
            Console.WriteLine("------------------------");
        }
        private static void PrintSignInMenu()
        {
            Console.WriteLine("Izaberite opciju:");
            Console.WriteLine("1. Prijava");
            Console.WriteLine("0 <-- EXIT");
        }
        private static void PrintOptionsMenu()
        {
            Console.WriteLine("Izaberite opciju:");
            Console.WriteLine("1. Upis vrednosti izlaznog taga");
            Console.WriteLine("2. Prikaz vrednosti izlaznih tagova");
            Console.WriteLine("3. Ukljucivanje/Iskljucivanje skeniranja ulaznih tagova");
            Console.WriteLine("4. Dodavanje ulaznog taga");
            Console.WriteLine("5. Dodavanje izlaznog taga");
            Console.WriteLine("6. Izmena izlaznog taga");
            Console.WriteLine("7. Brisanje ulaznog taga");
            Console.WriteLine("8. Brisanje izlaznog taga");
            Console.WriteLine("9. Odjava");
            if (role == "admin")
                Console.WriteLine("10. Registracija korisnika");
            Console.WriteLine("0 <-- EXIT");
        }
        private static bool IsEmpty(params string[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                //napraviti kasnije proveru za neke karaktere npr !?
                if (list[i].Trim() == "")
                    return true;
            }
            return false;
        }
        
        
        //static void deleteAlarm()
        //{
        //    Console.WriteLine("Tag name za alarm:");
        //    string tagName = Console.ReadLine();
        //    dbClient.deleteAlarm(tagName);
        //}
        //static void addAlarm()
        //{
        //    Console.WriteLine("Tag name za alarm:");
        //    string tagName = Console.ReadLine();
        //    Console.WriteLine("Tip: ");
        //    string type = Console.ReadLine();
        //    Console.WriteLine("Limit:");
        //    double limit;
        //    bool success = Double.TryParse(Console.ReadLine(), out limit);
        //    if(!success)
        //    {
        //        Console.WriteLine("Pogresan unos.");
        //        return;
        //    }
        //    int priority;
        //    bool successp = Int32.TryParse(Console.ReadLine(), out priority);
        //    if(!successp)
        //    {
        //        Console.WriteLine("Pogresan unos.");
        //        return;
        //    }
        //    dbClient.addAlarm(tagName, type, limit, priority, DateTime.Now);
        //}
        
        
        //static void editOutputTag()
        //{
        //    Console.WriteLine("Unesite id taga:");
        //    string tagName = Console.ReadLine();
        //    string tagType = dbClient.getOutputTag(tagName);
        //    if (tagType == null)
        //    {
        //        Console.WriteLine("Ne postoji ovaj tag");
        //        return;
        //    }

        //    Console.WriteLine("I/O Adresa:");
        //    string address = Console.ReadLine();

        //    dbClient.editOutputTag(tagName, address);

        //}
        
    }
}
