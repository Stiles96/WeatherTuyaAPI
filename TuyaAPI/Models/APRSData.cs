using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
    public class APRSData
    {
        public int rain { get; set; }
        public int temp { get; set; }
        public int wind_direct { get; set; }
        public int wind_speed_avg { get; set; }
        public int wind_speed_gust { get; set; }
        public int humidity { get; set; }
        public int pressure { get; set; }
    }
}
