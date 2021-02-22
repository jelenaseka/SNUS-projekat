using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScadaCore
{
    public static class RealTimeDriver
    {
        public static Dictionary<string, double> values = new Dictionary<string, double>();
        
        public static double ReturnValue(string address)
        {
            if (values.ContainsKey(address))
            {
                return values[address];
            }
            return 0;
        }
    }
}