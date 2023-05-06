using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
    public static class ClimateCommands
    {
        private static string[] _modes = new string[] { "cold",   "auto", "dehumidification", "wind_dry", "heat" };
        private static string[] _Fanmodes = new string[] { "mid","low", "auto",  "high" };
        public enum enModes
        {
            cold,
            auto  ,
            dehumidification,
            wind_dry,
            heat  
        }

        public enum enFanModes
        {
            mid,
            low,
            auto,
            high 
        }
        public static string getMode(enModes mode) { return _modes[(int)mode]; }
        public static string getFanMode(enFanModes mode) {
            
            return _Fanmodes[(int)mode]; }

    }
}
