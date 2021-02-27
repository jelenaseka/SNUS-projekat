using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportManager
{
    class Program
    {
        static ServiceReference.ReportManagerServiceClient reportManagerClient = new ServiceReference.ReportManagerServiceClient();
        static void Main(string[] args)
        {
            while(true)
            {
                OptionsMenu();
            }
        }
        private static void OptionsMenu()
        {
            PrintOptionsMenu();
            int option = ChoseOption();
            ExecuteOptionsMenu(option);
        }

        private static void PrintOptionsMenu()
        {
            Console.WriteLine("Izaberite opciju:");
            Console.WriteLine("1. Svi alarmi koji su se desili u odredjenom vremenskom periodu (sortiranje: prioritet, vreme)");
            Console.WriteLine("2. Svi alarmi odredjenog prioriteta (sortiranje: vreme)");
            Console.WriteLine("3. Sve vrednosti tagova koje su dospele na servis u odredjenom vremenskom periodu (sortiranje: vreme)");
            Console.WriteLine("4. Vrednosti svih AI tagova (sortiranje: vreme)");
            Console.WriteLine("5. Vrednosti svih DI tagova (sortiranje: vreme)");
            Console.WriteLine("6. Sve vrednosti taga sa odredjenim identifikatorom (sortiranje: vrednosti)");
            Console.WriteLine("0 <-- EXIT");
        }
        private static int ChoseOption()
        {
            bool success = false;
            int chosen = -1;
            while (!success)
            {
                success = Int32.TryParse(Console.ReadLine(), out chosen);
                if (!success)
                {
                    Console.WriteLine("Pogresan unos. Pokusajte ponovo");
                }
            }
            return chosen;
        }
        private static void ExecuteOptionsMenu(int option)
        {
            switch (option)
            {
                case 0: System.Environment.Exit(1); break;
                case 1: DisplayAlarmsByDate(); break;
                case 2: DisplayAlarmsByPriority(); break;
                case 3: DisplayTagsByDate(); break;
                case 4: DisplayAnalogInputs(); break;
                case 5: DisplayDigitalInputs(); break;
                case 6: DisplayTagById(); break;
                default: Console.WriteLine("Pogresan unos"); break;
            }
        }

        private static string GetSortType()
        {
            string sortType = Console.ReadLine();
            if (sortType == "1")
                return "asc";
            else if (sortType == "2")
                return "desc";
            else
            {
                return "error";
            }
        }
        private static void DisplayTagById()
        {
            Console.WriteLine("Unesite id taga:");
            string tagName = Console.ReadLine();
            Console.WriteLine("Sortiraj po vrednosti: 1. Rastuce, 2. Opadajuce");

            string sortType = GetSortType();
            if(sortType == "error")
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }

            string[] tags = reportManagerClient.DisplayTagById(tagName, sortType);
            DisplayTags(tags);
        }

        private static void DisplayAlarmsByDate()
        {
            Console.WriteLine("Unesite datum od (dd/MM/yyyy HH:mm AM/PM):");
            string dateFrom = Console.ReadLine();
            Console.WriteLine("Unesite datum do (dd/MM/yyyy HH:mm AM/PM):");
            string dateTo = Console.ReadLine();
            Console.WriteLine("Sortiraj po: 1. Prioritetu, 2. Vremenu");
            string sortParam = Console.ReadLine();

            if (sortParam == "1")
                sortParam = "priority";
            else if (sortParam == "2")
                sortParam = "time";
            else
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }

            Console.WriteLine("Sortiraj: 1. Rastuce, 2. Opadajuce");

            string sortType = GetSortType();
            if (sortType == "error")
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }

            DateTime dateFromValue;
            DateTime dateToValue;
            
            if (DateTime.TryParse(dateFrom, out dateFromValue) && DateTime.TryParse(dateTo, out dateToValue))
            {
                string[] alarms = reportManagerClient.DisplayAlarmsByDate(dateFromValue, dateToValue, sortParam, sortType);
                foreach (var alarm in alarms)
                    Console.WriteLine(alarm);
            }
            else
            {
                Console.WriteLine("Pogresan unos");
            }
        }


        private static void DisplayDigitalInputs()
        {
            Console.WriteLine("Sortiraj: 1. Rastuce, 2. Opadajuce");

            string sortType = GetSortType();
            if (sortType == "error")
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }

            string[] tags = reportManagerClient.DisplayDigitalInputs(sortType);
            DisplayTags(tags);
        }
        private static void DisplayAnalogInputs()
        {
            Console.WriteLine("Sortiraj: 1. Rastuce, 2. Opadajuce");

            string sortType = GetSortType();
            if (sortType == "error")
            {
                Console.WriteLine("Pogresan unos.");
                return;
            }
            string[] tags = reportManagerClient.DisplayAnalogInputs(sortType);
            DisplayTags(tags);
        }

        private static void DisplayAlarmsByPriority()
        {
            Console.WriteLine("Unesite prioritet: ");
            int priority;
            
            if (Int32.TryParse(Console.ReadLine(), out priority))
            {
                if(priority != 1 && priority != 2 && priority != 3)
                {
                    Console.WriteLine("Pogresan unos");
                    return;
                }
                Console.WriteLine("Sortiraj po vremenu: 1. Rastuce, 2. Opadajuce");

                string sortType = GetSortType();
                if (sortType == "error")
                {
                    Console.WriteLine("Pogresan unos.");
                    return;
                }
                string[] alarms = reportManagerClient.DisplayAlarmsByPriority(priority, sortType);
                foreach(var alarm in alarms)
                    Console.WriteLine(alarm);
            } else
            {
                Console.WriteLine("Pogresan unos.");
            }
        }

        private static void DisplayTagsByDate()
        {
            Console.WriteLine("Unesite datum od (dd/MM/yyyy HH:mm AM/PM):");
            string dateFrom = Console.ReadLine();
            Console.WriteLine("Unesite datum do (dd/MM/yyyy HH:mm AM/PM):");
            string dateTo = Console.ReadLine();

            DateTime dateFromValue;
            DateTime dateToValue;

            if (DateTime.TryParse(dateFrom, out dateFromValue) && DateTime.TryParse(dateTo, out dateToValue))
            {
                Console.WriteLine("Sortiraj: 1. Rastuce, 2. Opadajuce");
                string sortType = Console.ReadLine();
                if (sortType == "1")
                    sortType = "asc";
                else if (sortType == "2")
                    sortType = "desc";
                else
                {
                    Console.WriteLine("Pogresan unos.");
                    return;
                }
                string[] tags = reportManagerClient.DisplayTagsByDate(dateFromValue, dateToValue, sortType);
                DisplayTags(tags);
            } else
            {
                Console.WriteLine("Pogresan unos");
            }
        }

        private static void DisplayTags(string[] tags)
        {
            foreach (var tag in tags)
                Console.WriteLine(tag);
        }
    }
}
