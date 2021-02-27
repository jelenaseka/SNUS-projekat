using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace RealTimeUnit
{
    class Program
    {
        static ServiceReference.RealTimeUnitServiceClient rtuClient = new ServiceReference.RealTimeUnitServiceClient();
        private static CspParameters csp;
        private static RSACryptoServiceProvider rsa;
        const string EXPORT_FOLDER = @"C:\public_key\";
        const string PUBLIC_KEY_FILE = @"rsaPublicKey.txt";

        public static void CreateAsmKeys()
        {
            csp = new CspParameters();
            rsa = new RSACryptoServiceProvider(csp);
        }
        private static byte[] SignMessage(string message)
        {
            using (SHA256 sha = SHA256Managed.Create())
            {
                var hashValue = sha.ComputeHash(Encoding.UTF8.GetBytes(message));
                var formatter = new RSAPKCS1SignatureFormatter(rsa);
                formatter.SetHashAlgorithm("SHA256");
                return formatter.CreateSignature(hashValue);
            }
        }
        private static void ExportPublicKey()
        {
            //Kreiranje foldera za eksport ukoliko on ne postoji
            if (!(Directory.Exists(EXPORT_FOLDER)))
                Directory.CreateDirectory(EXPORT_FOLDER);
            string path = Path.Combine(EXPORT_FOLDER, PUBLIC_KEY_FILE);
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(rsa.ToXmlString(false));
            }
        }
        static void Main(string[] args)
        {

            Console.WriteLine("Registracija RTU");
            Console.WriteLine("Id: ");
            string id = Console.ReadLine();
            Console.WriteLine("Gornja granica:");
            int highLimit = ValidateInt(Console.ReadLine());
            Console.WriteLine("Donja granica:");
            int lowLimit = ValidateInt(Console.ReadLine());
            if (highLimit == -1 || lowLimit == -1 || lowLimit > highLimit)
            {
                Console.WriteLine("Pogresan unos.");
                Console.ReadKey();
                System.Environment.Exit(1);
            }

            Console.WriteLine("Adresa RTD:");
            string address = Console.ReadLine();

            CreateAsmKeys();
            ExportPublicKey();

            string message = id + address + lowLimit + highLimit;
            var signature = SignMessage(message);

            if (!rtuClient.Initialize(id, address, signature, message)) //provera poruke itd..
            {
                Console.WriteLine("Registracija neuspesna.");
                Console.ReadKey();
                System.Environment.Exit(1);

            }

            

            Random rnd = new Random();
            Console.WriteLine("Started working...");
            while (true)
            {
                
                rtuClient.SendValueToAddress(address,rnd.Next(lowLimit,highLimit));
                Thread.Sleep(5000);
            }
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
    }
}
