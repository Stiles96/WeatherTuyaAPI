using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
   public class BaseModel
    {
        public bool success { get; set; }
        public DateTimeOffset t { get; set; }
        public int code { get; set; }
        public string msg { get; set; }

    }
}
