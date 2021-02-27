using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography;

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
            string encryptedPassword = EncryptData(password);
            string role = dbClient.IsDatabaseEmpty() ? "admin" : "moderator";

            if (dbClient.Registration(username, encryptedPassword, role))
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
                Console.WriteLine("Morate popuniti sva polja.");
                return;
            }
            
            string response = dbClient.SignIn(username, password);
            if (response == "admin" || response == "moderator")
            {
                signedIn = true;
                role = response;
            }
            else Console.WriteLine(response);
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
                case 6: RemoveInputTag(); break;
                case 7: RemoveOutputTag(); break;
                case 8: AddAlarm(); break;
                case 9: DeleteAlarm(); break;
                case 10: SignOut(); break;
                case 11:
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
        private static void AddAlarm()
        {
            Console.WriteLine("Tag name za alarm:");
            string tagName = Console.ReadLine();
            Console.WriteLine("Tip(low/high): ");
            string type = Console.ReadLine();
            Console.WriteLine("Limit:");
            double limit = ValidateDouble(Console.ReadLine());
            Console.WriteLine("Priority:");
            int priority = ValidateInt(Console.ReadLine());

            if (!ValidateAlarmInput(type, limit, priority))
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }

            string response = dbClient.AddAlarm(tagName, type, limit, priority);
            Console.WriteLine(response);
        }
        private static void DeleteAlarm()
        {
            Console.WriteLine("Id alarma:");
            string id = Console.ReadLine();
            string response = dbClient.DeleteAlarm(id);
            Console.WriteLine(response);
        }

        private static double ValidateDouble(string number)
        {
            double doubleNumber;
            bool isDouble = Double.TryParse(number, out doubleNumber);
            if (isDouble)
            {
                return doubleNumber;
            }
            return -1;
        }
        private static int ValidateInt(string number)
        {
            int intNumber;
            bool isInt = Int32.TryParse(number, out intNumber);
            if (isInt)
            {
                return intNumber;
            }
            return -1;
        }
        private static bool ValidateAlarmInput(string type, double limit, int priority)
        {
            if ((type != "low" && type != "high") || limit == -1 || priority == -1)
                return false;
            
            if (priority != 1 && priority != 2 && priority != 3)
                return false;

            return true;
        }
        

        private static void RemoveOutputTag()
        {
            Console.WriteLine("Unesite id taga:");
            string tagName = Console.ReadLine();
            string response = dbClient.RemoveOutputTag(tagName);
            Console.WriteLine(response);
        }

        private static void RemoveInputTag()
        {
            Console.WriteLine("Unesite id taga:");
            string tagName = Console.ReadLine();
            string response = dbClient.RemoveInputTag(tagName);
            Console.WriteLine(response);
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
                if(initValue != 0 && initValue != 1)
                {
                    Console.WriteLine("Pocetna vrednost moze biti samo 0 ili 1.");
                    return;
                }
                AddDigitalOutput(tagName, description, address, initValue);
            }
        }

        private static void AddAnalogOutput(string tagName, string description, string address, double initValue)
        {
            Console.WriteLine("Low limit:");
            double lowLimit = ValidateDouble(Console.ReadLine());
            Console.WriteLine("High limit:");
            double highLimit = ValidateDouble(Console.ReadLine());
           
            if (lowLimit == -1 || highLimit == -1 || lowLimit > highLimit)
            {
                Console.WriteLine("Pogresan unos");
                return;
            }
            if(initValue > highLimit || initValue < lowLimit)
            {
                Console.WriteLine("Pocetna vrednost treba biti izmedju low i high limita.");
                return;
            }
            string response = dbClient.AddOutputTag(tagName, description, address, initValue, lowLimit, highLimit, "analog");
            Console.WriteLine(response);
        }

        private static void AddDigitalOutput(string tagName, string description, string address, double initValue)
        {
            string response = dbClient.AddOutputTag(tagName, description, address, initValue, 0, 0, "digital");
            Console.WriteLine(response);
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
            if(scanTime < 1)
            {
                Console.WriteLine("Scan time mora biti veci od 0");
                return;
            }
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
            string response = dbClient.AddInputTag(tagName, description, address, driver, scanTime, onOffScan, 0, 0, "digital");
            Console.WriteLine(response);
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
            string response = dbClient.AddInputTag(tagName, description, address, driver, scanTime, onOffScan, lowLimit, highLimit, "analog");
            Console.WriteLine(response);
        }

        private static void TurnScanOnOff()
        {
            Console.WriteLine("Unesite id ulaznog taga:");
            string tagName = Console.ReadLine();
            
            string response = dbClient.TurnScanOnOff(tagName);
            Console.WriteLine(response);
        }
        private static void GetOutputValues()
        {
            Dictionary<string, double> values = dbClient.GetOutputValues();
            
            foreach (KeyValuePair<string, double> kvp in values)
            {
                Console.WriteLine($"Address = {kvp.Key}, Value = {kvp.Value}");
            }
        }
        private static void ChangeOutputValue()
        {
            Console.WriteLine("Unesite id taga:");
            string tagName = Console.ReadLine();
            Console.WriteLine("Unesite novu vrednost:");
            double newValue = ValidateDouble(Console.ReadLine());
            
            if(newValue == -1)
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }

            string response = dbClient.ChangeOutputValue(tagName, newValue);
            Console.WriteLine(response);
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
            Console.WriteLine("6. Brisanje ulaznog taga");
            Console.WriteLine("7. Brisanje izlaznog taga");
            Console.WriteLine("8. Dodavanje alarma");
            Console.WriteLine("9. Brisanje alarma");
            Console.WriteLine("10. Odjava");
            if (role == "admin")
                Console.WriteLine("11. Registracija korisnika");
            Console.WriteLine("0 <-- EXIT");
        }
        private static bool IsEmpty(params string[] list)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Trim() == "")
                    return true;
            }
            return false;
        }

        private static string EncryptData(string valueToEncrypt)
        {
            string GenerateSalt()
            {
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                byte[] salt = new byte[32];
                crypto.GetBytes(salt);
                return Convert.ToBase64String(salt);
            }
            string EncryptValue(string strValue)
            {
                string saltValue = GenerateSalt();
                byte[] saltedPassword = Encoding.UTF8.GetBytes(saltValue + strValue);
                using (SHA256Managed sha = new SHA256Managed())
                {
                    byte[] hash = sha.ComputeHash(saltedPassword);
                    return $"{Convert.ToBase64String(hash)}:{saltValue}";
                }
            }
            return EncryptValue(valueToEncrypt);
        }
        

    }
}
