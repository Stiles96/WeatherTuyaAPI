using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
    public class Settings
    {
        public string BaseURL { get; set; } = Regions.URL_EU;
        public string Client_ID { get; set; }
        public string Client_Secret { get; set; }
        public string deviceId { get; set; }
        public string URL_InfluxDB { get; set; }
        public string API_Key { get; set; }
        public string Org { get; set; }
        public string Bucket { get; set; }
        public string Table { get; set; }
        public string sensor { get; set; }
        public string APRSUsername { get; set; }
        public string APRSPasscode { get; set; }
        public string APRSURL { get; set; } 
        public int APRSPort { get; set; } = 14580;
        public string Position { get; set; }
        public string APRSGateway { get; set; } = "rotate";
        public string MQTTUsername { get; set; }
        public string MQTTPassword { get; set; }
        public string MQTTHost { get; set; }
        public int MQTTPort { get; set; } = 1883;
        public string MQTTTopic { get; set; }
    }
}
