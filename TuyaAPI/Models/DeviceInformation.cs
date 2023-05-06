using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
    public class DeviceInformationModel
    {
        public class Result
        {
            public DateTimeOffset active_time { get; set; }
            public int biz_type { get; set; }
            public string category { get; set; }
            public DateTimeOffset create_time { get; set; }
            public string icon { get; set; }
            public string id { get; set; }
            public string ip { get; set; }
            public string lat { get; set; }
            public string local_key { get; set; }
            public string lon { get; set; }
            public string model { get; set; }
            public string name { get; set; }
            public string node_id { get; set; }
            public bool online { get; set; }
            public string owner_id { get; set; }
            public string product_id { get; set; }
            public string product_name { get; set; }
            public List<object> status { get; set; }
            public bool sub { get; set; }
            public string time_zone { get; set; }
            public string uid { get; set; }
            public DateTimeOffset update_time { get; set; }
            public string uuid { get; set; }
 
        }

        public class Device : BaseModel
        {
            public Result result { get; set; }

        }
    }
}
