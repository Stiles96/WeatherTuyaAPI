using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
   public class CommandModel
    {
        public class Command
        {
            public string code { get; set; }
            public object value { get; set; }
        }
        public class Commands
        {
            public List<Command> commands { get; set; }
        }
    }
}
