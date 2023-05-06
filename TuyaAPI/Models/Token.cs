using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
    public class TokenModel
    {

        public class Token : BaseModel
        {
            public Result result { get; set; }

        }

        public class Result
        {
            public string access_token { get; set; }
            public int expire_time { get; set; }
            public string refresh_token { get; set; }
            public string uid { get; set; }
            public DateTime ExpireDate { get; set; }
        }

    }
}
