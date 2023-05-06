using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
   public class DataTypes
    {
        public class Enum
        {
                public List<string> range { get; set; }
        }
        public class Integer
        {
            public int max { get; set; }
            public int min { get; set; }
            public int scale { get; set; }
            public int step { get; set; }
            public string type { get; set; }
            public string unit { get; set; }
        }
    }
}
