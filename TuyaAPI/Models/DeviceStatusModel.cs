using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
    public class DeviceStatusModel
    {
        public class Result
        {
            public string code { get; set; }
            public string value { get; set; }
        }

        public class DeviceStatus:BaseModel
        {
            public List<Result> result { get; set; }
        }

    }
}
