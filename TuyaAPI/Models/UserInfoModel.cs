using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
    public class UserInfoModel
    {

        public class UserInfo:BaseModel
        {
            public Result result { get; set; }
             
        }

        public class Result
        {
            public string avatar { get; set; }
            public string country_code { get; set; }
            public DateTimeOffset create_time { get; set; }
            public string email { get; set; }
            public string mobile { get; set; }
            public string nick_name { get; set; }
            public int temp_unit { get; set; }
            public string time_zone_id { get; set; }
            public string uid { get; set; }
            public DateTime update_time { get; set; }
            public string username { get; set; }
        }

    }
}
