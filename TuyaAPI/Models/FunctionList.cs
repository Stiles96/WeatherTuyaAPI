using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
    public class FunctionListModel
    {
        public class Function
        {
            public string code { get; set; }
            public string desc { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string values { get; set; }
        }

        public class Result
        {
            public string category { get; set; }
            public List<Function> functions { get; set; }
        }

        public class Functions : BaseModel
        {
            public Result result { get; set; }
 
        }
    }
}
