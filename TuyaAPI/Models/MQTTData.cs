using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
    public class MQTTData
    {
        public float indoor_temp { get; set; }
        public float outdoor_temp { get; set; }
        public int indoor_humidity { get; set; }
        public int outdoor_humidity { get; set; }
        public float rain { get; set; }
        public int wind_direct { get; set; }
        public int wind_speed_avg { get; set; }
        public int pressure { get; set; }
        public float dew_point { get; set; }
        public Int32 bright { get; set; }
        public bool online { get; set; }
        public DateTimeOffset LastUpdate { get; set; }
        public string IP { get; set; }

    }
}
