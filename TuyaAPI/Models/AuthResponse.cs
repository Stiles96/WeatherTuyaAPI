using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuyaAPI.Models
{
    public class AuthResponse
    {
        /// <summary>
        /// Access Token
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// Type of Access Token
        /// </summary>
        public string token_type { get; set; }
        /// <summary>
        /// Token expire Time
        /// </summary>
        public int expires_in { get; set; }
        /// <summary>
        /// Scope
        /// </summary>
        public string scope { get; set; }
        /// <summary>
        /// Get Default Token Header
        /// </summary>
        /// <returns>Access Token as Header</returns>
        public string GetHeader()
        {
            return token_type + " " + access_token;
        }
    }
}
